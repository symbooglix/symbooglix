using CommandLine;
using CommandLine.Text;
using System;
using System.IO;
using Microsoft;
using System.Linq;
using Microsoft.Boogie;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;



namespace Symbooglix
{
    public class driver
    {
        public class CmdLineOpts
        {
            [Option("append-query-log-file", DefaultValue = 0, HelpText = "When logging queries (see --log-queries) append to file rather than overwriting")]
            public int appendLoggedQueries { get; set; }

            [Option("dump-program", DefaultValue = "", HelpText = "Before execution write prepared program to file specified")]
            public string dumpProgramPath { get; set; }

            [Option("emit-before", DefaultValue = false, HelpText = "Emit Boogie program to stdout before running each pass")]
            public bool emitProgramBefore { get; set; }

            [Option("emit-after", DefaultValue = false, HelpText = "Emit Boogie program to stdout before running each pass")]
            public bool emitProgramAfter { get; set; }

            [OptionList('D', "defines",Separator = ',', HelpText="Add defines to the Boogie parser. Each define should be seperated by a comma.")]
            public List<string> Defines { get; set; }

            // FIXME: Urgh... how do you set the default value of the list?
            [OptionList('e', "entry-points",
                        Separator = ',',
                        DefaultValue = null,
                        HelpText = "Comma seperated list of implementations to use as entry points for execution.")]
            public List<string> entryPoints { get; set; }

            [Option("goto-assume-look-ahead", DefaultValue= 1, HelpText="Prevent needless state creation and destruction by looking ahead at gotos")]
            public int gotoAssumeLookAhead { get; set; }

            [Option("gpuverify-entry-points", DefaultValue=false, HelpText = "Use GPUVerify kernels as entry points")]
            public bool gpuverifyEntryPoints { get; set; }

            // FIXME: Booleans can't be disabled in the CommandLine library so use ints instead
            [Option("fold-constants", DefaultValue = 1, HelpText = "Use Constant folding during execution")]
            public int useConstantFolding { get; set; }

            [Option("human-readable-smtlib", DefaultValue = 1, HelpText = "When writing SMTLIBv2 queries make them more readable by using indentation and comments")]
            public int humanReadable { get; set ;}

            [Option("log-queries", DefaultValue = "", HelpText= "Path to file to log queries to. Blank means logging is disabled.")]
            public string queryLogPath { get; set; }

            [Option("print-instr", DefaultValue = false, HelpText = "Print instructions during execution")]
            public bool useInstructionPrinter { get; set; }

            [Option("print-call-seq", DefaultValue = false, HelpText = "Print call sequence during execution")]
            public bool useCallSequencePrinter { get; set; }

            [Option("timeout", DefaultValue=0, HelpText="Number of seconds to wait before killing executor for the current entry point")]
            public int timeout { get; set;}

            public enum Solver
            {
                CVC4,
                DUMMY,
                Z3
            }

            [Option("output-dir", DefaultValue="", HelpText="Directory to place Executor log files. By default a symbooglix-<N> directory is used")]
            public string outputDir { get; set; }

            // FIXME: The command line library should tell the user what are the valid values
            [Option("solver", DefaultValue = Solver.Z3, HelpText = "Solver to use (valid values CVC4, DUMMY, Z3)")]
            public Solver solver { get; set; }

            [Option("solver-path", DefaultValue = "", HelpText = "Path to the SMTLIBv2 solver")]
            public string pathToSolver { get; set; }

            [Option("solver-timeout", DefaultValue=0, HelpText="Maximum time allowed for a single query")]
            public int solverTimeout {get; set;}

            [Option("use-modset-transform", DefaultValue = 1, HelpText = "Run the modset analysis to fix incorrect modsets before type checking")]
            public int useModSetTransform { get; set; }

            // Positional args
            [ValueOption(0)]
            public string boogieProgramPath { get; set; }

            // For printing parser error messages
            [ParserState]
            public IParserState LastParserState { get; set; }

           
            [HelpOption]
            public string GetUsage()
            {
                var help = new HelpText {
                    Heading = new HeadingInfo("Symbooglix", "The symbolic execution engine for boogie programs"),
                    Copyright = new CopyrightInfo("Dan Liew", 2014),
                    AdditionalNewLineAfterOption = true,
                    AddDashesToOption = true
                };

                // FIXME: Printing parser errors is totally broken.
                if (LastParserState == null)
                    Console.WriteLine("FIXME: CommandLine parser did not give state");

                if (LastParserState != null && LastParserState.Errors.Any())
                {
                    var errors = help.RenderParsingErrorsText(this, 2);
                    help.AddPostOptionsLine("Error: Failed to parse command line options");
                    help.AddPostOptionsLine(errors);
                }
                else
                {

                    help.AddPreOptionsLine("Usage: symbooglix [options] <boogie program>");
                    help.AddOptions(this);
                }

                return help;
            }
        }

        public static int Main(String[] args)
        {
            // Debug log output goes to standard error.
            Debug.Listeners.Add(new ExceptionThrowingTextWritierTraceListener(Console.Error));

            // FIXME: Urgh... we are forced to use Boogie's command line
            // parser becaue the Boogie program resolver/type checker
            // is dependent on the parser being used...EURGH!
            CommandLineOptions.Install(new Microsoft.Boogie.CommandLineOptions());


            var options = new CmdLineOpts();
            if (! CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("Failed to parse args");
                return 1;
            }

            if (options.boogieProgramPath == null)
            {
                Console.WriteLine("A boogie program must be specified. See --help");
                return 1;
            }

            if (!File.Exists(options.boogieProgramPath))
            {
                Console.WriteLine("Boogie program \"" + options.boogieProgramPath + "\" does not exist");
                return 1;
            }

           
            Program p = null;

            if (options.Defines != null)
            {
                foreach (var define in options.Defines)
                    Console.WriteLine("Adding define \"" + define + "\" to Boogie parser");
            }

            int errors = Microsoft.Boogie.Parser.Parse(options.boogieProgramPath, options.Defines, out p);

            if (errors != 0)
            {
                Console.WriteLine("Failed to parse");
                return 1;
            }

            errors = p.Resolve();

            if (errors != 0)
            {
                Console.WriteLine("Failed to resolve.");
                return 1;
            }

            if (options.useModSetTransform > 0)
            {
                // This is useful for Boogie Programs produced by the GPUVerify tool that
                // have had instrumentation added that invalidates the modset attached to
                // procedures. By running the analysis we may modify the modsets attached to
                // procedures in the program to be correct so that Boogie's Type checker doesn't
                // produce an error.
                var modsetAnalyser = new ModSetCollector();
                modsetAnalyser.DoModSetAnalysis(p);
            }

            errors = p.Typecheck();

            if (errors != 0)
            {
                Console.WriteLine("Failed to Typecheck.");
                return 1;
            }


            IStateScheduler scheduler = new DFSStateScheduler();

            // Destroy the solver when we stop using it
            using (var solver = BuildSolverChain(options))
            {
                Executor e = new Executor(p, scheduler, solver);

                // Check all implementations exist and build list of entry points to execute
                var entryPoints = new List<Implementation>();

                // This is specific to GPUVerify
                if (options.gpuverifyEntryPoints)
                {
                    var kernels = p.TopLevelDeclarations.OfType<Implementation>().Where(impl => QKeyValue.FindBoolAttribute(impl.Attributes,"kernel"));
                    foreach (var kernel in kernels)
                    {
                        entryPoints.Add(kernel);
                    }

                    if (entryPoints.Count() == 0)
                    {
                        Console.WriteLine("Could not find any kernel entry points");
                        return 1;
                    }
                }
                else
                {
                    // Set main as default.
                    if (options.entryPoints == null)
                        options.entryPoints = new List<string>() { "main" };

                    foreach (var implString in options.entryPoints)
                    {
                        Implementation entry = p.TopLevelDeclarations.OfType<Implementation>().Where(i => i.Name == implString).FirstOrDefault();
                        if (entry == null)
                        {
                            Console.WriteLine("Could not find implementation \"" + implString + "\" to use as entry point");
                            return 1;
                        }
                        entryPoints.Add(entry);
                    }
                }

                if (options.useInstructionPrinter)
                {
                    Console.WriteLine("Installing instruction printer");
                    //e.RegisterPreEventHandler(new InstructionPrinter());
                    var instrPrinter = new InstructionPrinter(Console.Out);
                    instrPrinter.Connect(e);
                }

                if (options.useCallSequencePrinter)
                {
                    Console.WriteLine("Installing call sequence printer");
                    //e.RegisterPreEventHandler(new CallSequencePrinter());
                    var callPrinter = new CallPrinter(Console.Out);
                    callPrinter.Connect(e);
                }

                if (options.useConstantFolding > 0)
                {
                    e.UseConstantFolding = true;
                }
                else
                {
                    e.UseConstantFolding = false;
                }

                if (options.gotoAssumeLookAhead > 0)
                {
                    e.UseGotoLookAhead = true;
                }
                else
                {
                    e.UseGotoLookAhead = false;
                }

                // Just print a message about break points for now.
                e.BreakPointReached += BreakPointPrinter.handleBreakPoint;


                var stateHandler = new TerminationConsoleReporter();
                stateHandler.Connect(e);

                // Supply our own PassManager for preparation so we can hook into its events
                var PM = new Transform.PassManager(p);

                // Use anonymous methods so we can use closure to read command line options
                Transform.PassManager.PassRunEvent beforePassHandler = delegate(Object passManager, Transform.PassManager.PassManagerEventArgs eventArgs)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Running pass " + eventArgs.ThePass.GetName());
                    Console.ResetColor();
                    if (options.emitProgramBefore)
                    {
                        Console.WriteLine("**** Program before pass:");
                        Util.ProgramPrinter.Print(eventArgs.TheProgram, Console.Out, /*pretty=*/true, /*unstructured=*/ true);
                        Console.WriteLine("**** END Program before pass");
                    }
                };

                Transform.PassManager.PassRunEvent afterPassHandler = delegate(Object passManager, Transform.PassManager.PassManagerEventArgs eventArgs)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Finished running pass " + eventArgs.ThePass.GetName());
                    Console.ResetColor();
                    if (options.emitProgramAfter)
                    {
                        Console.WriteLine("**** Program after pass:");
                        Util.ProgramPrinter.Print(eventArgs.TheProgram, Console.Out, /*pretty=*/true, /*unstructured=*/ true);
                        Console.WriteLine("**** END Program after pass:");
                    }
                };

                var terminationCounter = new TerminationCounter();
                terminationCounter.Connect(e);

                PM.BeforePassRun += beforePassHandler;
                PM.AfterPassRun += afterPassHandler;

                ExecutorLogger executorLogger = null;
                if (options.outputDir.Length == 0)
                    executorLogger = new ExecutorLogger(Directory.GetCurrentDirectory(), /*makeDirectoryInPath=*/ true);
                else
                    executorLogger = new ExecutorLogger(options.outputDir, /*makeDirectoryInPath=*/ false);

                executorLogger.Connect(e);
                Console.WriteLine("Logging to directory: " + executorLogger.Root.FullName);

                e.PrepareProgram(PM);

                // Write program to file if requested
                if (options.dumpProgramPath.Length > 0)
                {
                    Console.Out.WriteLine("Writing prepared program to \"" + options.dumpProgramPath + "\"");
                    using (StreamWriter outputFile = File.CreateText(options.dumpProgramPath))
                    {
                        // FIXME: Get program from executor.
                        Util.ProgramPrinter.Print(p, outputFile, /*pretty=*/true, /*unstructured=*/ true);
                    }
                }

                foreach (var entryPoint in entryPoints)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Entering Implementation " + entryPoint.Name + " as entry point");
                    Console.ResetColor();
                    e.Run(entryPoint, options.timeout);
                }

                Console.WriteLine(solver.Statistics.ToString());
                Console.WriteLine(terminationCounter.ToString());
            }
            return 0;
        }

        public static Solver.ISolver BuildSolverChain(CmdLineOpts options)
        {
            Solver.ISolverImpl solverImpl = null;

            // Try to guess the location of executable. This is just for convenience
            if (options.pathToSolver.Length == 0 && options.solver != CmdLineOpts.Solver.DUMMY)
            {
                Console.WriteLine("Path to SMT solver not specified. Guessing location");

                // Look in the directory of the currently running executable for other solvers
                var pathToSolver = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                                                options.solver.ToString().ToLower());

                if (File.Exists(pathToSolver))
                {
                    Console.WriteLine("Found \"{0}\"", pathToSolver);
                    options.pathToSolver = pathToSolver;
                }
                else
                {
                    // Try with ".exe" appended
                    pathToSolver = pathToSolver + ".exe";
                    if (File.Exists(pathToSolver))
                    {
                        Console.WriteLine("Found \"{0}\"", pathToSolver);
                        options.pathToSolver = pathToSolver;
                    }
                    else
                    {
                        Console.Error.WriteLine("Could not find \"{0}\" (also without .exe)", pathToSolver);
                        System.Environment.Exit(1);
                    }
                }
            }

            switch (options.solver)
            {
                case CmdLineOpts.Solver.CVC4:
                    solverImpl = new Solver.CVC4SMTLIBSolver(options.pathToSolver);
                    break;
                case CmdLineOpts.Solver.Z3:
                    solverImpl = new Solver.Z3SMTLIBSolver(options.pathToSolver);
                    break;
                case CmdLineOpts.Solver.DUMMY:
                    solverImpl = new Solver.DummySolver(Symbooglix.Solver.Result.UNKNOWN);
                    break;
                default:
                    throw new NotSupportedException("Unhandled solver type");
            }



            if (options.queryLogPath.Length > 0)
            {
                // FIXME: How are we going to ensure this file gets closed properly?
                StreamWriter QueryLogFile = new StreamWriter(options.queryLogPath, /*append=*/ options.appendLoggedQueries > 0);
                solverImpl = new Solver.SMTLIBQueryLoggingSolverImpl(solverImpl, QueryLogFile, options.humanReadable > 0);
            }

            // Only support this for now.
            Solver.ISolver solver = new Solver.SimpleSolver(solverImpl);
            solver.SetTimeout(options.solverTimeout);
            return solver;
        }
    }
}

