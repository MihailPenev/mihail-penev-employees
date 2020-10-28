using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mihail_penev_employees
{
    public class Employment
    {
        public string EmpId { get; set; }
        public string ProjectID { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
    public class Employee
    {
        public string EmpId { get; set; }
        public List<Employment> Projects { get; set; }

        public TimeSpan CoworkingExperienceWith(Employee employee2)
        {
            TimeSpan daysWorkingTogether = TimeSpan.FromSeconds(0);
            foreach (var projectEmployee1 in Projects)
            {
                foreach (var projectEmployee2 in employee2.Projects)
                {
                    //If both employees worked on the same project at the same time
                    if (projectEmployee1.DateFrom <= projectEmployee2.DateTo && projectEmployee2.DateFrom <= projectEmployee1.DateTo &&
                        projectEmployee1.ProjectID == projectEmployee2.ProjectID)
                    {
                        //calculate how long the two employees worked together
                        //there are 4 possible ways how two time intervals can intersect
                        if (projectEmployee1.DateFrom >= projectEmployee2.DateFrom && projectEmployee1.DateTo <= projectEmployee2.DateTo) daysWorkingTogether += projectEmployee1.DateTo - projectEmployee1.DateFrom;
                        else if (projectEmployee1.DateFrom >= projectEmployee2.DateFrom && projectEmployee1.DateTo >= projectEmployee2.DateTo) daysWorkingTogether += projectEmployee2.DateTo - projectEmployee1.DateFrom;
                        else if (projectEmployee1.DateFrom <= projectEmployee2.DateFrom && projectEmployee1.DateTo >= projectEmployee2.DateTo) daysWorkingTogether += projectEmployee2.DateTo - projectEmployee2.DateFrom;
                        else daysWorkingTogether += projectEmployee1.DateTo - projectEmployee2.DateFrom;
                    }
                }
            }
            return daysWorkingTogether;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //didn't want to overcomplicate the solution using Singleton for the pair with the most experice,
            //that's why I'm using the following variables to store the IDs of the two employees with most experience working together 
            string pairEmployeeId1 = "";
            string pairEmployeeId2 = "";
            int totalPairCoworkingDays = 0;

            Console.WriteLine(@"Please enter full path and file name, eg. ""D:\myFiles\Data.txt""");
            string filePath = Console.ReadLine();
            while (!File.Exists(filePath))
            {
                Console.WriteLine("There is no such file. Please try again.");
                filePath = Console.ReadLine();
            }

            //dump the file data into the memory and give it structure so it's easier to work with
            List<Employment> employments = new List<Employment>();
            string line;
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(filePath);
                while ((line = file.ReadLine()) != null)
                {
                    var values = line.Split(", ");
                    //Last column can have "NULL" string value. It should be replaced with today's date in yyyy-MM-dd format.
                    if (values[3] == "NULL") values[3] = DateTime.Now.ToString("yyyy-MM-dd");
                    employments.Add(
                        new Employment
                        {
                            EmpId = values[0],
                            ProjectID = values[1],
                            DateFrom = DateTime.Parse(values[2]),
                            DateTo = DateTime.Parse(values[3])
                        });
                }
                //data is already in the memory
                file.Close();
            }
            catch
            {
                Console.WriteLine("File could not be open.");
                Console.ReadKey();
                return;
            }
            
            

            //Because I'm looking for an employee pair, I'm grouping all projects together under the same Employee. Which I can later use to make employee pairs.
            var employees = from e in employments
                         group e by e.EmpId into g
                         select new Employee(){ EmpId = g.Key, Projects = g.ToList() };

            //forming pairs and finding the one with most experience at the same time.
            foreach (var employee1 in employees)
            {
                foreach(var employee2 in employees)
                {
                    if(employee1.EmpId != employee2.EmpId)
                    {
                        int pairCoworkingDays = employee1.CoworkingExperienceWith(employee2).Days;
                        //Console.WriteLine(employee1.EmpId + " worked together with " + employee2.EmpId + " for " + pairCoworkingDays + " days");
                        //check if current pair has more experience than the one know as the most experienced
                        if (pairCoworkingDays > totalPairCoworkingDays)
                        {
                            //make current pair the most experienced one
                            totalPairCoworkingDays = pairCoworkingDays;
                            pairEmployeeId1 = employee1.EmpId;
                            pairEmployeeId2 = employee2.EmpId;
                        }
                    }
                }
            }
            Console.Clear();
            Console.WriteLine("The team with the most experience working together is \nEmpID: " + pairEmployeeId1 + " x EmpID: " + pairEmployeeId2);
            Console.WriteLine("With total of " + totalPairCoworkingDays + " days.");
            Console.ReadKey();
        }
    }
}
