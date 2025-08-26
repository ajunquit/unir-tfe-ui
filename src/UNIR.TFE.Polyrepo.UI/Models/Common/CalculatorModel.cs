namespace UNIR.TFE.Polyrepo.UI.Models.Common
{
    public class CalculatorModel: CommonModel
    {
        public decimal Number1 { get; set; }
        public decimal Number2 { get; set; }
        public decimal Result { get; set; }
        public string Operation { get; set; } = "Addition"; // Default operation
        public GitModel GitModel { get; set; } = new();
    }
}
