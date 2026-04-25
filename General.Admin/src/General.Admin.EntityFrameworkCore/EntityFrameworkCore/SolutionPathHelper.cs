using System.IO;

namespace General.Admin.EntityFrameworkCore;

public static class SolutionPathHelper
{
    public static string FindSolutionRoot(string startDirectory)
    {
        var current = new DirectoryInfo(startDirectory);

        while (current != null)
        {
            if (current.GetFiles("*.sln").Any() || current.GetFiles("*.slnx").Any())
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return startDirectory;
    }
}
