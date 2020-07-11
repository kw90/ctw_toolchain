include "globals.mzn";

int: k;  % number of cavities = length of the permutation
set of int: Positions = 1..k;
set of int: Cavities = Positions;

int: b;  % number of bothside wired cavity-pairs
set of int: CavityPairs = 1..2*b;
set of int: CableStarts = 1..b;

array[int,int] of Cavities: AtomicConstraints;
array[int,int] of Cavities: DisjunctiveConstraints;
array[int] of Cavities: DirectSuccessors;
array[int,int] of Cavities: SoftAtomicConstraints;

array[Cavities] of var Positions: pfc;
constraint all_different (pfc);

% atomic constraints
constraint
forall (i in index_set_1of2(AtomicConstraints))
  (pfc[AtomicConstraints[i,1]] < pfc[AtomicConstraints[i,2]]);

% disjunctive constraints
array[int] of var bool: disj = 
  [pfc[DisjunctiveConstraints[i, 2*j+1]] < pfc[DisjunctiveConstraints[i,2*j+2]]
      | i in index_set_1of2(DisjunctiveConstraints), j in 0..1
  ];
constraint forall (i in 1..length(disj) div 2) (disj[(i-1)*2+1] \/ disj[(i-1)*2+2]);
  
% direct successor constraints
constraint
forall (i in index_set(DirectSuccessors))
  (if DirectSuccessors[i]<= b
    then pfc[DirectSuccessors[i]] < pfc[DirectSuccessors[i]+b]
      -> pfc[DirectSuccessors[i]] +1 = pfc[DirectSuccessors[i]+b]
    else pfc[DirectSuccessors[i]] < pfc[DirectSuccessors[i]-b]
      -> pfc[DirectSuccessors[i]] +1 = pfc[DirectSuccessors[i]-b]
  endif);

% Criterium S
array[int] of var bool: vio_abs =
  [pfc[i] - pfc[i+b] > 1 \/ pfc[i+b] - pfc[i] > 1 | i in CableStarts];
   
var int: S = 
  if b=0 then 0
  else sum(vio_abs)
  endif;
  
    
var int: M = 
  if b=0 then 0
  else (max(i in CavityPairs) (
        sum(j in CavityPairs where j <= b)
            (pfc[j] < pfc[i] /\ pfc[i] < pfc[j+b])
          + sum(j in CavityPairs where j > b)
            (pfc[j] < pfc[i] /\ pfc[i] < pfc[j-b])))
  endif;
  
% Criterium L
var int: diff1 = 
  if b=0 then 0
  else (max(i in CableStarts)(pfc[i] - pfc[i+b]))
  endif;
var int: diff2 = 
  if b=0 then 0
  else (max(i in CableStarts)(pfc[i+b] - pfc[i]))
  endif;
var int: L =
  if b=0 then 0
  else (max(diff1, diff2) - 1)
  endif;
 
 % Criterium N
 array[int] of var bool: vio =
   [pfc[SoftAtomicConstraints[i,1]] >= pfc[SoftAtomicConstraints[i,2]]
     | i in index_set_1of2(SoftAtomicConstraints)];
     
var int: N = sum(vio);

var int: obj = S * pow(k, 3) + 
               M * pow(k, 2) + 
               L * pow(k, 1) + 
               N;


solve minimize obj;

output ["\([S,M,L,N])\n\(obj)\n\(pfc)\n"];
