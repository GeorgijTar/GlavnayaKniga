using GlavnayaKniga.Domain.Entities;
using System;
using System.Collections.Generic;

namespace GlavnayaKniga.Application.Helpers
{
    public static class NomenclatureHelper
    {
        private static readonly Dictionary<NomenclatureType, string> _typeToRussian = new()
        {
            { NomenclatureType.Material, "Материалы" },
            { NomenclatureType.Inventory, "Инвентарь" },
            { NomenclatureType.Fertilizer, "Удобрения" },
            { NomenclatureType.PlantProtection, "СЗР" },
            { NomenclatureType.Seeds, "Семена" },
            { NomenclatureType.Fuel, "Топливо" },
            { NomenclatureType.SpareParts, "Запчасти" },
            { NomenclatureType.Equipment, "Оборудование" },
             { NomenclatureType.Work, "Работы" },          
            { NomenclatureType.Service, "Услуги" },       
            { NomenclatureType.Other, "Прочее" }
        };

        private static readonly Dictionary<string, NomenclatureType> _russianToType = new()
        {
            { "Материалы", NomenclatureType.Material },
            { "Инвентарь", NomenclatureType.Inventory },
            { "Удобрения", NomenclatureType.Fertilizer },
            { "СЗР", NomenclatureType.PlantProtection },
            { "Семена", NomenclatureType.Seeds },
            { "Топливо", NomenclatureType.Fuel },
            { "Запчасти", NomenclatureType.SpareParts },
            { "Оборудование", NomenclatureType.Equipment },
            { "Работы", NomenclatureType.Work },         
            { "Услуги", NomenclatureType.Service },
            { "Прочее", NomenclatureType.Other }
        };

       
        public static string GetRussianType(NomenclatureType type)
        {
            return _typeToRussian.TryGetValue(type, out var russian) ? russian : "Прочее";
        }

        public static NomenclatureType GetTypeFromRussian(string russian)
        {
            return _russianToType.TryGetValue(russian, out var type) ? type : NomenclatureType.Other;
        }

                public static List<string> GetAllRussianTypes()
        {
            return new List<string>(_typeToRussian.Values);
        }
    }
}