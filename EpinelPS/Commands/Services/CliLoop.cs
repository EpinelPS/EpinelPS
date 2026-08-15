namespace EpinelPS.Commands.Services;

public static class CliLoop
{
    private static readonly string promptPostfix = "# ";

    public static void Start()
    {
        Console.WriteLine("EpinelPS CLI");
        Console.WriteLine("NOTICE: Admin panel is available at https://localhost/admin/");

        var manager = new Manager();
        string prompt = promptPostfix;

        while (true)
        {
            Console.Write(prompt);

            string? input = Console.ReadLine();
            if (input == null || input == string.Empty) continue;

            var result = manager.ExecuteCommandAsync(input).GetAwaiter().GetResult();
            if (result.Color.HasValue) Console.ForegroundColor = result.Color.Value;
            Console.WriteLine(result.Message);
            Console.ResetColor();

            prompt = manager.UserName != null
                ? $"/{manager.UserName ?? "Unknown"}" + promptPostfix
                : promptPostfix;
        }
    }
}
