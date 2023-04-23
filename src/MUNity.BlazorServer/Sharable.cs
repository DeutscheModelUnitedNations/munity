using Radzen;

namespace MUNity.BlazorServer
{
    public static class Sharable
    {
        public static readonly ConfirmOptions YesNoOptions = new ConfirmOptions()
        {
            OkButtonText = "Ja",
            CancelButtonText = "Nein"
        };
    }
}
