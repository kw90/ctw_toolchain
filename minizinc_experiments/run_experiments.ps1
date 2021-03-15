# Windows PowerShell Script to solve calbe tree instances
# - place model files Mz1.mzn and Mz2.mzn in the folder /models
# - and all instances to the folder /data
# the following parameters can be set:
$solvers=@("cplex", "gurobi") # the cplex mip solver is used as default in minizinc
$models=@("models\Mz1.mzn","models\Mz2.mzn", "models\Mz1-noAbs-dualmodel.mzn")
$nbThreads=1
$timeLimit=300000

$instances= (Get-ChildItem -LiteralPath "data").FullName

# loop over all solvers, all models and instances
for($i=0; $i -lt $solvers.Length; $i++){
    for($j=0; $j -lt $models.Length; $j++){
       
        for($k=0; $k -lt $instances.Length; $k++){
            
             Write-Output "Solver: $($solvers[$i]) with model $($models[$j]) and instance $($instances[$k])"
            .\minizinc.exe --solver $solvers[$i] -f -p $nbThreads --solver-time-limit $timeLimit --solver-statistics $models[$j] $instances[$k]
        }

   
    }
}