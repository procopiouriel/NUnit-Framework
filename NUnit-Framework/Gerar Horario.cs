using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.OrTools.Sat;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class GerarHorario
{
    public static bool Error = false;

    public bool ReturnVolta()
    {
        employee_scheduling();
        if (Error == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void employee_scheduling()
    {

        const int numNurses = 4;
        const int numDays = 3;
        const int numShifts = 3;

        int[] allNurses = Enumerable.Range(0, numNurses).ToArray();
        int[] allDays = Enumerable.Range(0, numDays).ToArray();
        int[] allShifts = Enumerable.Range(0, numShifts).ToArray();

        CpModel model = new CpModel();
        model.Model.Variables.Capacity = numNurses * numDays * numShifts;
        Dictionary<(int, int, int), BoolVar> shifts =
            new Dictionary<(int, int, int), BoolVar>(numNurses * numDays * numShifts);
        foreach (int n in allNurses)
        {
            foreach (int d in allDays)
            {
                foreach (int s in allShifts)
                {
                    shifts.Add((n, d, s), model.NewBoolVar($"shifts_n{n}d{d}s{s}"));
                }
            }
        }

        List<ILiteral> literals = new List<ILiteral>();
        foreach (int d in allDays)
        {
            foreach (int s in allShifts)
            {
                foreach (int n in allNurses)
                {
                    literals.Add(shifts[(n, d, s)]);
                }
                model.AddExactlyOne(literals);
                literals.Clear();
            }
        }

        foreach (int n in allNurses)
        {
            foreach (int d in allDays)
            {
                foreach (int s in allShifts)
                {
                    literals.Add(shifts[(n, d, s)]);
                }
                model.AddAtMostOne(literals);
                literals.Clear();
            }
        }

        int minShiftsPerNurse = (numShifts * numDays) / numNurses;
        int maxShiftsPerNurse;
        if ((numShifts * numDays) % numNurses == 0)
        {
            maxShiftsPerNurse = minShiftsPerNurse;
        }
        else
        {
            maxShiftsPerNurse = minShiftsPerNurse + 1;
        }

        List<IntVar> shiftsWorked = new List<IntVar>();
        foreach (int n in allNurses)
        {
            foreach (int d in allDays)
            {
                foreach (int s in allShifts)
                {
                    shiftsWorked.Add(shifts[(n, d, s)]);
                }
            }
            model.AddLinearConstraint(LinearExpr.Sum(shiftsWorked), minShiftsPerNurse, maxShiftsPerNurse);
            shiftsWorked.Clear();
        }

        CpSolver solver = new CpSolver();
        // Tell the solver to enumerate all solutions.
        solver.StringParameters += "linearization_level:0 " + "enumerate_all_solutions:true ";

        const int solutionLimit = 5;
        SolutionPrinter cb = new SolutionPrinter(allNurses, allDays, allShifts, shifts, solutionLimit);

        CpSolverStatus status = solver.Solve(model, cb);
        Console.WriteLine($"Solve status: {status}");

    }
}





public class SolutionPrinter : CpSolverSolutionCallback
{
    public SolutionPrinter(int[] allNurses, int[] allDays, int[] allShifts,
                           Dictionary<(int, int, int), BoolVar> shifts, int limit)
    {
        solutionCount_ = 1;
        allNurses_ = allNurses;
        allDays_ = allDays;
        allShifts_ = allShifts;
        shifts_ = shifts;
        solutionLimit_ = limit;
    }



    public override void OnSolutionCallback()
    {

        int[] enfermeirosTrabalhando = new int[3];
        int enfermeiroFolga = -1;
        for (int i = 0; i < enfermeirosTrabalhando.Length; i++)//LIMPAR ARRAY
        {

            enfermeirosTrabalhando[i] = -1;
        }
        Console.WriteLine($"Solution #{solutionCount_}:");
        foreach (int d in allDays_)
        {
            Console.WriteLine($"\n\nDay {d}");
            foreach (int n in allNurses_)
            {
                bool isWorking = false;
                foreach (int s in allShifts_)
                {


                    if (Value(shifts_[(n, d, s)]) == 1L)
                    {
                        isWorking = true;

                        for (int i = 0; i < enfermeirosTrabalhando.Length; i++)
                        {
                            if (enfermeirosTrabalhando[i] == -1)
                            {
                                enfermeirosTrabalhando[i] = n;
                                break;

                            }
                        }
                        Console.WriteLine($"  O enfermeiro {n} trabalha no turno {s}");

                    }


                }
                if (!isWorking)
                {
                    enfermeiroFolga = d;
                    Console.WriteLine($"  Nurse {d} does not work");


                }
                /* for (int i = 0; i < enfermeirosTrabalhando.Length; i++)
                 {
                    //Console.WriteLine("Enfermeiro: " + enfermeirosTrabalhando[i]);
                 }*/

            }
            for (int i = 0; i < enfermeirosTrabalhando.Length; i++)
            {
                if (enfermeirosTrabalhando[i] == enfermeiroFolga)
                {
                    Console.WriteLine("====Houve erro!====");
                    //GerarHorario.Error = true;
                }
            }
            for (int i = 0; i < enfermeirosTrabalhando.Length; i++)//LIMPAR ARRAY
            {
                enfermeirosTrabalhando[i] = -1;
            }

        }


        solutionCount_++;
        if (solutionCount_ >= solutionLimit_)
        {
            Console.WriteLine($"Stop search after {solutionLimit_} solutions");
            StopSearch();
        }
    }

    public int SolutionCount()
    {
        return solutionCount_;
    }

    private int solutionCount_;
    private int[] allNurses_;
    private int[] allDays_;
    private int[] allShifts_;
    private Dictionary<(int, int, int), BoolVar> shifts_;
    private int solutionLimit_;
}