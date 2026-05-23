namespace FinamConnection.Core
{
    /// <summary>
    /// Портфель счёта — баланс и список позиций.
    /// Структура соответствует ответу GET /v1/accounts/{account_id}
    /// </summary>
    public class Portfolio
    {
        /// <summary>Номер счёта</summary>
        public string AccountId { get; set; } = string.Empty;

        /// <summary>Тип счёта: FORTS (срочный рынок) или MOEX (фондовый рынок)</summary>
        public string AccountType { get; set; } = string.Empty;

        /// <summary>Статус счёта: ACCOUNT_ACTIVE и др.</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Общая стоимость портфеля (equity)</summary>
        public decimal Equity { get; set; }

        /// <summary>Свободные денежные средства</summary>
        public decimal Cash { get; set; }

        /// <summary>Зарезервированные средства (под открытые позиции)</summary>
        public decimal MoneyReserved { get; set; }

        /// <summary>Нереализованная прибыль/убыток по открытым позициям</summary>
        public decimal UnrealizedProfit { get; set; }

        /// <summary>Стоимость всех позиций</summary>
        public decimal PositionsValue { get; set; }

        /// <summary>Список открытых позиций</summary>
        public List<Position> Positions { get; set; } = new();
    }

    /// <summary>
    /// Одна позиция в портфеле
    /// </summary>
    public class Position
    {
        /// <summary>Тикер инструмента, например SBER</summary>
        public string Symbol { get; set; } = string.Empty;

        /// <summary>Название инструмента</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Количество лотов</summary>
        public decimal Quantity { get; set; }

        /// <summary>Средняя цена покупки</summary>
        public decimal AveragePrice { get; set; }

        /// <summary>Текущая цена</summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>Текущая стоимость позиции</summary>
        public decimal CurrentValue { get; set; }

        /// <summary>Нереализованная прибыль/убыток в рублях</summary>
        public decimal PnL { get; set; }

        /// <summary>Прибыль/убыток в процентах</summary>
        public decimal PnLPercent => AveragePrice > 0
            ? Math.Round((CurrentPrice - AveragePrice) / AveragePrice * 100, 2)
            : 0;
    }
}
