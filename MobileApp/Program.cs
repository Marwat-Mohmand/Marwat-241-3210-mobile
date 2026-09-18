// Console.WriteLine("C# is working!");

// Console.Write("Enter your name: ");
// string? name = Console.ReadLine();

// Console.WriteLine($"Hello, {name}!");


Console.ForegroundColor = ConsoleColor.White;
Console.BackgroundColor = ConsoleColor.Red;
Console.WriteLine(" White text on RED background ");

Console.ForegroundColor = ConsoleColor.Black;
Console.BackgroundColor = ConsoleColor.Yellow;
Console.WriteLine(" Black text on YELLOW background ");

Console.ForegroundColor = ConsoleColor.White;
Console.BackgroundColor = ConsoleColor.Blue;
Console.WriteLine(" White text on BLUE background ");

Console.ForegroundColor = ConsoleColor.Black;
Console.BackgroundColor = ConsoleColor.Green;
Console.WriteLine(" Black text on GREEN background ");

Console.ResetColor();
Console.WriteLine("Normal colors again");

Console.ForegroundColor = ConsoleColor.White;
Console.BackgroundColor = ConsoleColor.DarkBlue;
Console.WriteLine(" White on Dark Blue ");

Console.ForegroundColor = ConsoleColor.Yellow;
Console.BackgroundColor = ConsoleColor.DarkRed;
Console.WriteLine(" Yellow on Dark Red ");

Console.ForegroundColor = ConsoleColor.Black;
Console.BackgroundColor = ConsoleColor.Cyan;
Console.WriteLine(" Black on Cyan ");

Console.ForegroundColor = ConsoleColor.White;
Console.BackgroundColor = ConsoleColor.DarkGreen;
Console.WriteLine(" White on Dark Green ");

Console.ResetColor();