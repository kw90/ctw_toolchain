# Windows Powershell script to solve cable tree instances with the Minizinc command line tool

## Instructions

* Install the Minizinc IDE, CPLEX and Gurobi 
* Place the content of this repository into the folder, where Minizinc was installed (location of th `minizinc.exe`)
* Change parameters `$solvers` and `$models` in the script
* execute the script ` .\run_experiments.ps1 >> output.txt`

The script runs over all `$models`, `$solvers` and instances found in the folder `data`