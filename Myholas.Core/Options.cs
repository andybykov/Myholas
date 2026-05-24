namespace Myholas.Core
{
    public class Options
    {
        // инициируем Connection string
        public static string ConnectionString
        {
            get
            {
                // Получаем данные Connection string из переменных среды
                string name = "ConnectionStringMyholas";
                string? cs = Environment.GetEnvironmentVariable(name);

                if (cs == null)
                {
                    throw new InvalidOperationException($"Environment variable {name} is not set or empty");
                }

                return cs;
            }
        }
    }
}
