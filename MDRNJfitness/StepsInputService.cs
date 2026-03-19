using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace FitnessApp;

public static class StepsInputService
{
    // Prompts user to enter steps for the selected day
    public static async Task<int?> PromptForSteps(int dayNumber, int currentSteps = 0)
    {
        string result = await Application.Current.MainPage.DisplayPromptAsync(
            $"Day {dayNumber} Steps",
            "Enter steps taken for the day:",
            initialValue: currentSteps > 0 ? currentSteps.ToString() : "",
            keyboard: Keyboard.Numeric);

        if (int.TryParse(result, out int steps))
            return steps;

        return null;
    }
}
