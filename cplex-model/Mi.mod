int k  = ...; // number of cavities, permutation length
int b = ...;  // number of wired cavity pairs

range Cavities = 1..k;
range Cablestarts = 1 ..b;

// atomic constraints
tuple Atomic{ int cbefore; int cafter;};

// disjunctive constraints
tuple Disjun{
	int c1before;
    int c1after;
    int c2before; 
    int c2after;  
};

{Atomic} AtomicConstraints = ...;
{Atomic} SoftAtomicConstraints = ...;
{Disjun} DisjunctiveConstraints = ...;

{int} DirectSuccessors = ...;

 
range Positions = 1..k;
range CavityPairs = 1..2*b ;

// positionforchamber
dvar int pfc[Cavities] in Positions;

//OPTIMIZATION CRITERIA

dexpr int S = ((b == 0) ? 0	: sum(j in Cavities: 1 <= j <= b)
		(maxl(pfc[j], pfc[j+b]) - minl(pfc[j], pfc[j+b]) >= 2));

//dexpr float L =	((b == 0) ? 0 : max (j in Cablestarts) ((abs(pfc[j] - pfc[j+b])) - 1));
dexpr int L = ((b == 0) ? 0 : max (j in Cablestarts) (maxl(pfc[j] - pfc[j+b], pfc[j+b] - pfc[j]) - 1));

dexpr int M = ((b == 0) ? 0	: (max (i in CavityPairs)
		(sum(j in CavityPairs: j <= b) (pfc[j] <= (pfc[i] - 1) && pfc[i] <= (pfc[j+b] - 1)) 
		 + sum(j in CavityPairs: j > b) (pfc[j] <= (pfc[i] - 1) && pfc[i] <= (pfc[j-b] - 1)))));
		 
dexpr int N = sum(s in SoftAtomicConstraints)(pfc[s.cafter] - pfc[s.cbefore] <= 0);

minimize S * pow(k, 3)
		   + M * pow(k, 2)
		   + L * pow(k, 1) 
		   + N;

		      
subject to {
	
	//alldifferent
	forall(i, j in Cavities: i != j)
	     pfc[i] != pfc[j];
	
	//atomic constraints
	forall(c in AtomicConstraints)
	    pfc[c.cbefore] - pfc[c.cafter]  <= 0;        
		
	//disjunctive constraints
	forall(d in DisjunctiveConstraints)
	 (pfc[d.c1before] -  pfc[d.c1after] <= 0 ||  pfc[d.c2before] -  pfc[d.c2after]  <= 0 );
	
	// direct successor
	forall (i in DirectSuccessors: i <= b)  
	 pfc[i] - pfc[i+b] <= 0 =>  pfc[i+b] == pfc[i] + 1;
	
    forall (i in DirectSuccessors: i > b)  
	 pfc[i] - pfc[i-b] <= 0 =>  pfc[i-b] == pfc[i] + 1;
		
  
}
