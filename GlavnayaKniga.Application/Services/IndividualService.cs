using GlavnayaKniga.Application.DTOs;
using GlavnayaKniga.Application.Interfaces;
using GlavnayaKniga.Domain.Common;
using GlavnayaKniga.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlavnayaKniga.Application.Services
{
    public class IndividualService : IIndividualService
    {
        private readonly IRepository<Individual> _individualRepository;

        public IndividualService(IRepository<Individual> individualRepository)
        {
            _individualRepository = individualRepository;
        }

        public async Task<IEnumerable<IndividualDto>> GetAllIndividualsAsync(bool includeArchived = false)
        {
            var individuals = await _individualRepository.FindAsync(i => includeArchived || !i.IsArchived);

            return individuals
                .OrderBy(i => i.LastName)
                .ThenBy(i => i.FirstName)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<IndividualDto?> GetIndividualByIdAsync(int id)
        {
            var individual = await _individualRepository.GetByIdAsync(id);
            return individual != null ? MapToDto(individual) : null;
        }

        public async Task<IndividualDto?> GetIndividualByINNAsync(string inn)
        {
            if (string.IsNullOrWhiteSpace(inn)) return null;

            var individuals = await _individualRepository.FindAsync(i => i.INN == inn);
            var individual = individuals.FirstOrDefault();

            return individual != null ? MapToDto(individual) : null;
        }

        public async Task<IndividualDto?> GetIndividualBySNILSAsync(string snils)
        {
            if (string.IsNullOrWhiteSpace(snils)) return null;

            var individuals = await _individualRepository.FindAsync(i => i.SNILS == snils);
            var individual = individuals.FirstOrDefault();

            return individual != null ? MapToDto(individual) : null;
        }

        public async Task<IEnumerable<IndividualDto>> SearchIndividualsAsync(string searchText, bool includeArchived = false)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllIndividualsAsync(includeArchived);

            var searchLower = searchText.ToLower();
            var individuals = await _individualRepository.FindAsync(i =>
                (includeArchived || !i.IsArchived) &&
                (i.LastName.ToLower().Contains(searchLower) ||
                 i.FirstName.ToLower().Contains(searchLower) ||
                 (i.MiddleName != null && i.MiddleName.ToLower().Contains(searchLower)) ||
                 (i.Phone != null && i.Phone.Contains(searchText)) ||
                 (i.Email != null && i.Email.ToLower().Contains(searchLower)) ||
                 (i.INN != null && i.INN.Contains(searchText)) ||
                 (i.SNILS != null && i.SNILS.Contains(searchText))));

            return individuals
                .OrderBy(i => i.LastName)
                .ThenBy(i => i.FirstName)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<IndividualDto> CreateIndividualAsync(IndividualDto individualDto)
        {
            // Проверка уникальности ИНН
            if (!string.IsNullOrWhiteSpace(individualDto.INN))
            {
                if (!await IsINNUniqueAsync(individualDto.INN))
                {
                    throw new InvalidOperationException($"Физическое лицо с ИНН {individualDto.INN} уже существует");
                }
            }

            // Проверка уникальности СНИЛС
            if (!string.IsNullOrWhiteSpace(individualDto.SNILS))
            {
                if (!await IsSNILSUniqueAsync(individualDto.SNILS))
                {
                    throw new InvalidOperationException($"Физическое лицо с СНИЛС {individualDto.SNILS} уже существует");
                }
            }

            var individual = new Individual
            {
                LastName = individualDto.LastName,
                FirstName = individualDto.FirstName,
                MiddleName = individualDto.MiddleName,
                BirthDate = individualDto.BirthDate,
                BirthPlace = individualDto.BirthPlace,
                Gender = individualDto.Gender,
                Citizenship = individualDto.Citizenship,
                RegistrationAddress = individualDto.RegistrationAddress,
                ActualAddress = individualDto.ActualAddress,
                Phone = individualDto.Phone,
                Email = individualDto.Email,
                PassportSeries = individualDto.PassportSeries,
                PassportNumber = individualDto.PassportNumber,
                PassportIssueDate = individualDto.PassportIssueDate,
                PassportIssuedBy = individualDto.PassportIssuedBy,
                PassportDepartmentCode = individualDto.PassportDepartmentCode,
                INN = individualDto.INN,
                SNILS = individualDto.SNILS,
                Note = individualDto.Note,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _individualRepository.AddAsync(individual);
            return MapToDto(created);
        }

        public async Task<IndividualDto> UpdateIndividualAsync(IndividualDto individualDto)
        {
            var individual = await _individualRepository.GetByIdAsync(individualDto.Id);
            if (individual == null)
            {
                throw new InvalidOperationException($"Физическое лицо с ID {individualDto.Id} не найдено");
            }

            // Проверка уникальности ИНН (если изменился)
            if (individual.INN != individualDto.INN && !string.IsNullOrWhiteSpace(individualDto.INN))
            {
                if (!await IsINNUniqueAsync(individualDto.INN, individualDto.Id))
                {
                    throw new InvalidOperationException($"Физическое лицо с ИНН {individualDto.INN} уже существует");
                }
            }

            // Проверка уникальности СНИЛС (если изменился)
            if (individual.SNILS != individualDto.SNILS && !string.IsNullOrWhiteSpace(individualDto.SNILS))
            {
                if (!await IsSNILSUniqueAsync(individualDto.SNILS, individualDto.Id))
                {
                    throw new InvalidOperationException($"Физическое лицо с СНИЛС {individualDto.SNILS} уже существует");
                }
            }

            individual.LastName = individualDto.LastName;
            individual.FirstName = individualDto.FirstName;
            individual.MiddleName = individualDto.MiddleName;
            individual.BirthDate = individualDto.BirthDate;
            individual.BirthPlace = individualDto.BirthPlace;
            individual.Gender = individualDto.Gender;
            individual.Citizenship = individualDto.Citizenship;
            individual.RegistrationAddress = individualDto.RegistrationAddress;
            individual.ActualAddress = individualDto.ActualAddress;
            individual.Phone = individualDto.Phone;
            individual.Email = individualDto.Email;
            individual.PassportSeries = individualDto.PassportSeries;
            individual.PassportNumber = individualDto.PassportNumber;
            individual.PassportIssueDate = individualDto.PassportIssueDate;
            individual.PassportIssuedBy = individualDto.PassportIssuedBy;
            individual.PassportDepartmentCode = individualDto.PassportDepartmentCode;
            individual.INN = individualDto.INN;
            individual.SNILS = individualDto.SNILS;
            individual.Note = individualDto.Note;
            individual.UpdatedAt = DateTime.UtcNow;

            await _individualRepository.UpdateAsync(individual);
            return MapToDto(individual);
        }

        public async Task<bool> ArchiveIndividualAsync(int id)
        {
            var individual = await _individualRepository.GetByIdAsync(id);
            if (individual == null) return false;

            individual.IsArchived = true;
            individual.ArchivedAt = DateTime.UtcNow;
            individual.UpdatedAt = DateTime.UtcNow;

            await _individualRepository.UpdateAsync(individual);
            return true;
        }

        public async Task<bool> UnarchiveIndividualAsync(int id)
        {
            var individual = await _individualRepository.GetByIdAsync(id);
            if (individual == null || !individual.IsArchived) return false;

            individual.IsArchived = false;
            individual.ArchivedAt = null;
            individual.UpdatedAt = DateTime.UtcNow;

            await _individualRepository.UpdateAsync(individual);
            return true;
        }

        public async Task<bool> DeleteIndividualAsync(int id)
        {
            var individual = await _individualRepository.GetByIdAsync(id);
            if (individual == null) return false;

            // TODO: Проверить, есть ли связанные сотрудники

            await _individualRepository.DeleteAsync(individual);
            return true;
        }

        public async Task<bool> IsINNUniqueAsync(string inn, int? excludeId = null)
        {
            var individuals = await _individualRepository.FindAsync(i => i.INN == inn);

            if (excludeId.HasValue)
            {
                return !individuals.Any(i => i.Id != excludeId.Value);
            }

            return !individuals.Any();
        }

        public async Task<bool> IsSNILSUniqueAsync(string snils, int? excludeId = null)
        {
            var individuals = await _individualRepository.FindAsync(i => i.SNILS == snils);

            if (excludeId.HasValue)
            {
                return !individuals.Any(i => i.Id != excludeId.Value);
            }

            return !individuals.Any();
        }

        private IndividualDto MapToDto(Individual individual)
        {
            return new IndividualDto
            {
                Id = individual.Id,
                LastName = individual.LastName,
                FirstName = individual.FirstName,
                MiddleName = individual.MiddleName,
                BirthDate = individual.BirthDate,
                BirthPlace = individual.BirthPlace,
                Gender = individual.Gender,
                Citizenship = individual.Citizenship,
                RegistrationAddress = individual.RegistrationAddress,
                ActualAddress = individual.ActualAddress,
                Phone = individual.Phone,
                Email = individual.Email,
                PassportSeries = individual.PassportSeries,
                PassportNumber = individual.PassportNumber,
                PassportIssueDate = individual.PassportIssueDate,
                PassportIssuedBy = individual.PassportIssuedBy,
                PassportDepartmentCode = individual.PassportDepartmentCode,
                INN = individual.INN,
                SNILS = individual.SNILS,
                Note = individual.Note,
                IsArchived = individual.IsArchived,
                CreatedAt = individual.CreatedAt,
                UpdatedAt = individual.UpdatedAt
            };
        }
    }
}