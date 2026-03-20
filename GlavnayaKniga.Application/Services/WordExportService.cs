using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// Используем псевдонимы для избежания конфликтов имен
using WPDocument = DocumentFormat.OpenXml.Wordprocessing.Document;
using WPParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WPRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WPText = DocumentFormat.OpenXml.Wordprocessing.Text;
using WPTable = DocumentFormat.OpenXml.Wordprocessing.Table;
using WPTableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using WPTableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;
using WPBold = DocumentFormat.OpenXml.Wordprocessing.Bold;
using WPStyle = DocumentFormat.OpenXml.Wordprocessing.Style;

namespace GlavnayaKniga.Application.Services
{
    public class WordExportService : IWordExportService
    {
        public async Task<byte[]> ExportAccountsToWordAsync(IEnumerable<AccountDto> accounts, string title = "План счетов")
        {
            using var stream = new MemoryStream();
            using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
            {
                // Добавляем основную часть документа
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new WPDocument();
                var body = mainPart.Document.AppendChild(new Body());

                // Добавляем стили
                AddStyles(mainPart);

                // Добавляем заголовок
                AddTitle(body, title);

                // Добавляем таблицу
                AddAccountsTable(body, accounts);

                mainPart.Document.Save();
            }

            stream.Position = 0;
            return stream.ToArray();
        }

        public async Task<string> SaveAccountsToWordFileAsync(IEnumerable<AccountDto> accounts, string filePath, string title = "План счетов")
        {
            var data = await ExportAccountsToWordAsync(accounts, title);
            await File.WriteAllBytesAsync(filePath, data);
            return filePath;
        }

        private void AddStyles(MainDocumentPart mainPart)
        {
            var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
            stylesPart.Styles = new Styles();

            // Стиль для заголовка
            var titleStyle = new WPStyle
            {
                Type = StyleValues.Paragraph,
                StyleId = "TitleStyle",
                CustomStyle = true
            };
            titleStyle.Append(new Name { Val = "TitleStyle" });
            titleStyle.Append(new BasedOn { Val = "Normal" });
            titleStyle.Append(new NextParagraphStyle { Val = "Normal" });
            titleStyle.Append(new ParagraphProperties
            {
                Justification = new Justification { Val = JustificationValues.Center },
                SpacingBetweenLines = new SpacingBetweenLines { After = "200" }
            });
            titleStyle.Append(new RunProperties
            {
                Bold = new WPBold(),
                FontSize = new FontSize { Val = "28" },
                Color = new Color { Val = "2E74B5" }
            });

            // Стиль для заголовков таблицы
            var tableHeaderStyle = new WPStyle
            {
                Type = StyleValues.Paragraph,
                StyleId = "TableHeaderStyle",
                CustomStyle = true
            };
            tableHeaderStyle.Append(new Name { Val = "TableHeaderStyle" });
            tableHeaderStyle.Append(new BasedOn { Val = "Normal" });
            tableHeaderStyle.Append(new RunProperties
            {
                Bold = new WPBold(),
                FontSize = new FontSize { Val = "22" }
            });

            // Стиль для обычного текста
            var normalStyle = new WPStyle
            {
                Type = StyleValues.Paragraph,
                StyleId = "Normal",
                Default = true
            };
            normalStyle.Append(new Name { Val = "Normal" });
            normalStyle.Append(new RunProperties
            {
                FontSize = new FontSize { Val = "20" }
            });

            stylesPart.Styles.Append(titleStyle);
            stylesPart.Styles.Append(tableHeaderStyle);
            stylesPart.Styles.Append(normalStyle);
        }

        private void AddTitle(Body body, string title)
        {
            var titleParagraph = new WPParagraph();
            var titleRun = new WPRun();
            titleRun.AppendChild(new WPText(title));
            titleParagraph.AppendChild(titleRun);
            titleParagraph.ParagraphProperties = new ParagraphProperties
            {
                Justification = new Justification { Val = JustificationValues.Center },
                SpacingBetweenLines = new SpacingBetweenLines { After = "200" }
            };
            body.AppendChild(titleParagraph);
        }

        private void AddAccountsTable(Body body, IEnumerable<AccountDto> accounts)
        {
            // Создаем таблицу
            var table = new WPTable();

            // Настройка ширины таблицы
            var tableWidth = new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct };
            var tableProperties = new TableProperties();
            tableProperties.Append(tableWidth);
            tableProperties.Append(new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 2 },
                new BottomBorder { Val = BorderValues.Single, Size = 2 },
                new LeftBorder { Val = BorderValues.Single, Size = 2 },
                new RightBorder { Val = BorderValues.Single, Size = 2 },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 1 },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 1 }
            ));
            table.AppendChild(tableProperties);

            // Добавляем заголовок таблицы
            AddTableHeader(table);

            // Добавляем строки со счетами
            AddAccountRows(table, accounts, 0);

            body.AppendChild(table);
        }

        private void AddTableHeader(WPTable table)
        {
            var headerRow = new WPTableRow();

            // Ячейка для кода
            var codeCell = new WPTableCell();
            codeCell.Append(new TableCellProperties(
                new TableCellWidth { Width = "1000", Type = TableWidthUnitValues.Dxa },
                new Shading { Fill = "D9E1F2" }
            ));
            var codeParagraph = new WPParagraph();
            var codeRun = new WPRun();
            codeRun.AppendChild(new WPBold());
            codeRun.AppendChild(new WPText("Код"));
            codeParagraph.AppendChild(codeRun);
            codeCell.AppendChild(codeParagraph);
            headerRow.AppendChild(codeCell);

            // Ячейка для наименования
            var nameCell = new WPTableCell();
            nameCell.Append(new TableCellProperties(
                new TableCellWidth { Width = "4000", Type = TableWidthUnitValues.Dxa },
                new Shading { Fill = "D9E1F2" }
            ));
            var nameParagraph = new WPParagraph();
            var nameRun = new WPRun();
            nameRun.AppendChild(new WPBold());
            nameRun.AppendChild(new WPText("Наименование"));
            nameParagraph.AppendChild(nameRun);
            nameCell.AppendChild(nameParagraph);
            headerRow.AppendChild(nameCell);

            table.AppendChild(headerRow);
        }

        private void AddAccountRows(WPTable table, IEnumerable<AccountDto> accounts, int level)
        {
            foreach (var account in accounts)
            {
                var row = new WPTableRow();

                // Ячейка с кодом (с отступом)
                var codeCell = new WPTableCell();
                var codeParagraph = new WPParagraph();
                var codeRun = new WPRun();

                // Добавляем отступ в зависимости от уровня
                if (level > 0)
                {
                    codeRun.AppendChild(new WPText(new string(' ', level * 2) + account.Code));
                }
                else
                {
                    codeRun.AppendChild(new WPText(account.Code));
                }

                codeParagraph.AppendChild(codeRun);
                codeCell.AppendChild(codeParagraph);
                row.AppendChild(codeCell);

                // Ячейка с наименованием
                var nameCell = new WPTableCell();
                var nameParagraph = new WPParagraph();
                var nameRun = new WPRun();
                nameRun.AppendChild(new WPText(account.Name));
                nameParagraph.AppendChild(nameRun);
                nameCell.AppendChild(nameParagraph);
                row.AppendChild(nameCell);

                table.AppendChild(row);

                // Добавляем дочерние счета
                if (account.Children != null && account.Children.Any())
                {
                    AddAccountRows(table, account.Children, level + 1);
                }
            }
        }
    }
}