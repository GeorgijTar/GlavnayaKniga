using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Services
{
    public class BankStatementParser : IBankStatementParser
    {
        public async Task<BankStatementParseResult> ParseFileAsync(string filePath)
        {
            try
            {
                // Пробуем разные кодировки
                string content = await ReadFileWithMultipleEncodingsAsync(filePath);
                var fileName = Path.GetFileName(filePath);
                return ParseContent(content, fileName);
            }
            catch (Exception ex)
            {
                return new BankStatementParseResult
                {
                    Success = false,
                    ErrorMessage = $"Ошибка чтения файла: {ex.Message}",
                    FileName = Path.GetFileName(filePath)
                };
            }
        }

        private async Task<string> ReadFileWithMultipleEncodingsAsync(string filePath)
        {
            // Список кодировок для пробования
            var encodings = new[]
            {
                Encoding.GetEncoding(1251), // windows-1251
                Encoding.UTF8,
                Encoding.GetEncoding(866), // DOS (альтернативная кириллица)
                Encoding.Default, // Системная кодировка по умолчанию
                Encoding.ASCII
            };

            List<Exception> exceptions = new List<Exception>();

            foreach (var encoding in encodings)
            {
                try
                {
                    return await File.ReadAllTextAsync(filePath, encoding);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    // Продолжаем пробовать следующую кодировку
                }
            }

            // Если ни одна кодировка не сработала, пробуем прочитать без указания кодировки
            try
            {
                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            throw new Exception($"Не удалось прочитать файл ни в одной из кодировок. Последняя ошибка: {exceptions.LastOrDefault()?.Message}");
        }

        public BankStatementParseResult ParseContent(string content, string fileName)
        {
            var result = new BankStatementParseResult
            {
                Success = true,
                FileName = fileName,
                Documents = new List<BankStatementDocumentDto>(),
                DailyInfo = new Dictionary<DateTime, StatementDayInfo>()
            };

            try
            {
                var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                var currentSection = string.Empty;
                var currentDocument = new Dictionary<string, string>();
                var inDocument = false;
                var statementStartFound = false;
                var versionFound = false;
                var currentDate = DateTime.MinValue;
                var currentDayInfo = new StatementDayInfo();

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                    // Проверяем формат файла
                    if (trimmedLine == "1CClientBankExchange")
                    {
                        statementStartFound = true;
                        continue;
                    }

                    if (trimmedLine.StartsWith("ВерсияФормата="))
                    {
                        var version = trimmedLine.Substring("ВерсияФормата=".Length);
                        if (version != "1.03" && version != "1.02")
                        {
                            result.Success = false;
                            result.ErrorMessage = $"Неподдерживаемая версия формата: {version}. Ожидается 1.03";
                            return result;
                        }
                        versionFound = true;
                        continue;
                    }

                    if (!statementStartFound || !versionFound) continue;

                    // Обработка секций
                    if (trimmedLine.StartsWith("СекцияРасчСчет"))
                    {
                        currentSection = "РасчСчет";
                        continue;
                    }

                    if (trimmedLine == "КонецРасчСчет")
                    {
                        currentSection = string.Empty;

                        // Сохраняем информацию о дне, если она есть
                        if (currentDate != DateTime.MinValue)
                        {
                            result.DailyInfo[currentDate] = new StatementDayInfo
                            {
                                Date = currentDate,
                                OpeningBalance = currentDayInfo.OpeningBalance,
                                TotalIncoming = currentDayInfo.TotalIncoming,
                                TotalOutgoing = currentDayInfo.TotalOutgoing,
                                ClosingBalance = currentDayInfo.ClosingBalance
                            };
                            currentDate = DateTime.MinValue;
                            currentDayInfo = new StatementDayInfo();
                        }
                        continue;
                    }

                    if (trimmedLine.StartsWith("СекцияДокумент="))
                    {
                        currentSection = "Документ";
                        inDocument = true;
                        currentDocument = new Dictionary<string, string>();
                        currentDocument["DocumentType"] = trimmedLine.Substring("СекцияДокумент=".Length);
                        continue;
                    }

                    if (trimmedLine == "КонецДокумента")
                    {
                        if (currentDocument.Any())
                        {
                            var document = ParseDocument(currentDocument);
                            if (document != null)
                            {
                                result.Documents.Add(document);

                                // Обновляем дневную информацию
                                var docDate = document.Date.Date;
                                if (!result.DailyInfo.ContainsKey(docDate))
                                {
                                    result.DailyInfo[docDate] = new StatementDayInfo { Date = docDate };
                                }

                                if (document.IsIncoming)
                                {
                                    result.DailyInfo[docDate].TotalIncoming += document.Amount;
                                }
                                else
                                {
                                    result.DailyInfo[docDate].TotalOutgoing += document.Amount;
                                }
                            }
                        }
                        inDocument = false;
                        currentDocument.Clear();
                        continue;
                    }

                    if (trimmedLine == "КонецФайла")
                    {
                        break;
                    }

                    // Парсим строки вида Ключ=Значение
                    var equalIndex = trimmedLine.IndexOf('=');
                    if (equalIndex > 0)
                    {
                        var key = trimmedLine.Substring(0, equalIndex);
                        var value = trimmedLine.Substring(equalIndex + 1);

                        if (currentSection == "РасчСчет" && !inDocument)
                        {
                            ParseStatementInfo(key, value, result, ref currentDate, ref currentDayInfo);
                        }
                        else if (inDocument)
                        {
                            currentDocument[key] = value;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new BankStatementParseResult
                {
                    Success = false,
                    ErrorMessage = $"Ошибка парсинга: {ex.Message}",
                    FileName = fileName
                };
            }
        }

        private void ParseStatementInfo(string key, string value, BankStatementParseResult result, ref DateTime currentDate, ref StatementDayInfo currentDayInfo)
        {
            switch (key)
            {
                case "РасчСчет":
                    result.AccountNumber = value;
                    break;
                case "ДатаНачала":
                    if (DateTime.TryParseExact(value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                        result.StartDate = startDate;
                    break;
                case "ДатаКонца":
                    if (DateTime.TryParseExact(value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                        result.EndDate = endDate;
                    break;
                case "Дата":
                    if (DateTime.TryParseExact(value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    {
                        currentDate = date;
                        currentDayInfo.Date = date;
                    }
                    break;
                case "НачальныйОстаток":
                    if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var openingBalance))
                        currentDayInfo.OpeningBalance = openingBalance;
                    break;
                case "ВсегоПоступило":
                    if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var totalIncoming))
                        currentDayInfo.TotalIncoming = totalIncoming;
                    break;
                case "ВсегоСписано":
                    if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var totalOutgoing))
                        currentDayInfo.TotalOutgoing = totalOutgoing;
                    break;
                case "КонечныйОстаток":
                    if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var closingBalance))
                        currentDayInfo.ClosingBalance = closingBalance;
                    break;
            }
        }

        private BankStatementDocumentDto? ParseDocument(Dictionary<string, string> documentData)
        {
            try
            {
                var doc = new BankStatementDocumentDto();

                doc.DocumentType = documentData.GetValueOrDefault("DocumentType", "Неизвестно");
                doc.Number = documentData.GetValueOrDefault("Номер", "");

                if (documentData.TryGetValue("Дата", out var dateStr))
                {
                    if (DateTime.TryParseExact(dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                        doc.Date = date;
                }

                if (documentData.TryGetValue("Сумма", out var amountStr))
                {
                    if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                        doc.Amount = amount;
                }

                // Данные плательщика
                doc.PayerAccount = documentData.GetValueOrDefault("ПлательщикСчет", "");
                doc.PayerINN = documentData.GetValueOrDefault("ПлательщикИНН", "");
                doc.PayerName = documentData.GetValueOrDefault("Плательщик1", "");
                doc.PayerBIK = documentData.GetValueOrDefault("ПлательщикБИК", "");

                // Данные получателя
                doc.RecipientAccount = documentData.GetValueOrDefault("ПолучательСчет", "");
                doc.RecipientINN = documentData.GetValueOrDefault("ПолучательИНН", "");
                doc.RecipientName = documentData.GetValueOrDefault("Получатель1", "");
                doc.RecipientBIK = documentData.GetValueOrDefault("ПолучательБИК", "");

                // Данные платежа
                doc.PaymentPurpose = documentData.GetValueOrDefault("НазначениеПлатежа", "");
                doc.PaymentType = documentData.GetValueOrDefault("ВидПлатежа", "");

                if (documentData.TryGetValue("Очередность", out var priorityStr))
                {
                    if (int.TryParse(priorityStr, out var priority))
                        doc.Priority = priority;
                }

                // Даты поступления/списания
                if (documentData.TryGetValue("ДатаПоступило", out var receivedStr))
                {
                    if (DateTime.TryParseExact(receivedStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var received))
                        doc.ReceivedDate = received;
                }

                if (documentData.TryGetValue("ДатаСписано", out var withdrawnStr))
                {
                    if (DateTime.TryParseExact(withdrawnStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var withdrawn))
                        doc.WithdrawnDate = withdrawn;
                }

                // Определяем направление (входящий или исходящий)
                // Если есть ДатаПоступило - это входящий платеж
                // Если есть ДатаСписано - это исходящий платеж
                doc.IsIncoming = doc.ReceivedDate.HasValue;

                // Создаем хэш для обнаружения дубликатов
                doc.Hash = CreateDocumentHash(doc);

                return doc;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка парсинга документа: {ex.Message}");
                return null;
            }
        }

        private string CreateDocumentHash(BankStatementDocumentDto doc)
        {
            using var sha256 = SHA256.Create();
            var hashInput = $"{doc.DocumentType}|{doc.Number}|{doc.Date:yyyyMMdd}|{doc.Amount}|{doc.PayerAccount}|{doc.RecipientAccount}";
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hashInput));
            return Convert.ToHexString(bytes);
        }
    }
}