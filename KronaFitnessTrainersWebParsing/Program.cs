using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Xml.Linq; // Работаем с LINQ to XML
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using KronaFitnessTrainersWebParsing.Models;


public class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        XDocument document = GetPageAsXDocument("https://www.kronafitness.ru/uslugi/");
        document.Save("trainers_doc.xml");

        var trainers = new List<Trainer>();

        List<string> trainerNames = document.Descendants().
            Where(e => e.Attribute("class")?.Value == "cl-ib-title")
            .Select(e => e.Value).ToList();
        Console.WriteLine($"Загружено тренеров: {trainerNames.Count}");

        foreach (string name in trainerNames)
        {
            trainers.Add(new Trainer
            {
                Name = name,
            });
        }
        List<string> trainerHrefs = document.Descendants()
            .Where(e =>
            {
                bool con1 = e.Attribute("class")?.Value == "et_pb_text_inner";
                bool con2 = e.Elements().Any(e => e.Name.LocalName == "a");
                return con1 && con2;
            })
            .Select(e =>
            {
                var href = e.Elements().FirstOrDefault().Attribute("href").Value;
                return href;
            }).ToList();
        Console.WriteLine($"Загружено ссылок: {trainerHrefs.Count}");

        for (int i = 0; i < trainers.Count; i++)
        {
            trainers[i].InfoHref = trainerHrefs[i];
        }

        //PrintTrainers(trainers);

        TrainerParsing(trainers[0]);
        //Console.WriteLine(string.Join("\n", trainers[0].Specializations));
        Console.WriteLine(trainers[0].Education);
    }
    public static XDocument GetPageAsXDocument(string url)
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");

        using (IWebDriver driver = new ChromeDriver(options))
        {
            driver.Navigate().GoToUrl(url);
            Thread.Sleep(3000);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(driver.PageSource);
            htmlDoc.OptionOutputAsXml = true;

            // Используем XDocument.Parse вместо XmlDocument.LoadXml
            return XDocument.Parse(htmlDoc.DocumentNode.OuterHtml);
        }
    }

    public static void PrintTrainers(List<Trainer> trainers)
    {
        foreach (var trainer in trainers)
        {
            Console.WriteLine($"{trainer.Name,25}\t|\t{trainer.InfoHref}");
        }
    }

    public static void TrainerParsing(Trainer trainer)
    {
        var document = GetPageAsXDocument(trainer.InfoHref);



        trainer.Specializations = GetTrainerSpecialisationList(document);
        trainer.Education = GetTrainerEducation(document);
    }
    
    public static List<string> GetTrainerSpecialisationList(XDocument document)
    {
        var specialisations = document.Descendants()
            //если элемент имеет класс "et_pb_text_inner" и включает в себя h1
            .Where(e => e.Attribute("class")?.Value == "et_pb_text_inner" && e.Elements().Any(ee => ee.Name.LocalName == "h1"))
            .Select(e =>
            {
                string specialis = e.Elements().First(ee => ee.Name.LocalName == "h5").Value;
                return specialis;
            })
            .First()
            .Split("— ")
            .ToList()
            ;

        return specialisations;
    }

    public static string GetTrainerEducation(XDocument document)
    {
        var education = document.Descendants()
            //если элемент имеет класс "et_pb_text_inner" и включает в себя h1
            .Where(e => e.Attribute("class")?.Value == "et_pb_text_inner" && e.Elements().Any(ee => ee.Name.LocalName == "strong"))
            .Select(e =>
            {
                string educ = e.Elements().First().Value;
                return educ;
            })
            .First()
            .Split("Образование — ")
            [1]
            ;

        return education;
    }
}