using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;

public static class AutomationExamples
{
    public static void MoveToGmail(IAppLogging logging)
    {
        logging.LogMessage("MoveToGmail");
    
        foreach (AutomationElement el in AllMainWindows)
        {
            var name =
                (string) el.GetCurrentPropertyValue(AutomationElement
                    .NameProperty);
            if (name.EndsWith("Google Chrome"))
            {
                logging.LogMessage($"Found element {name}");
                
                el.SetFocus();
                Thread.Sleep(1000);
                SendKeys.SendWait("i");
                break;
            }
            // PrintGoogleChromeTabNames(el, logging);
                // logging.LogMessage(name);
            // WindowPattern xxwinPattern =
            //     (WindowPattern) el.GetCurrentPattern(WindowPattern
            //         .Pattern);
            // TextPattern textPattern =
            //     (TextPattern) el.GetCurrentPattern(TextPattern
            //         .Pattern);
        }
    }

    private static AutomationElementCollection AllMainWindows =>
        AutomationElement.RootElement.FindAll(
            TreeScope.Children, Condition.TrueCondition);

    private static void PrintGoogleChromeTabNames(AutomationElement el,
        IAppLogging logging)
    {
        var chromeElements =
            el.FindAll(TreeScope.Children, Condition.TrueCondition);
        foreach (AutomationElement childEl in chromeElements)
        {
            var name =
                (string) childEl.GetCurrentPropertyValue(AutomationElement
                    .NameProperty);
            logging.LogMessage(name);            
        }
    }
}