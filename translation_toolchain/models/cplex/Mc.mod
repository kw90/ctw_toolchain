using CP;

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

// positionforchamber pfc, chamberforposition cfp
dvar int pfc[Cavities] in Positions;
dvar int cfp[Positions] in Cavities;

//OPTIMIZATION CRITERIA

dexpr int S = (b == 0) ? 0 : (sum(i in Cablestarts) (abs(pfc[i] - pfc[i+b]) > 1));

dexpr int M = (b == 0) ? 0 :
	(max(i in CavityPairs) 
		(sum(j in CavityPairs: j<=b) ((pfc[j] < pfc[i] && pfc[i] < pfc[j+b]) ? 1 : 0) 
		 + sum(j in CavityPairs: j>b) ((pfc[j] < pfc[i] && pfc[i] < pfc[j-b]) ? 1 : 0)));

dexpr float L = (b == 0) ? 0 : max(i in Cablestarts) abs(pfc[i] - pfc[i+b]) - 1;  

dexpr int N = sum(i in SoftAtomicConstraints) (pfc[i.cbefore] > pfc[i.cafter]);


minimize S * pow(k, 3)
			+ M * pow(k, 2)
			+ L * pow(k, 1) 
			+  N;
		 
		   
constraints {

	 allDifferent(pfc);
	 allDifferent(cfp);
	//channel constraints:
 	 inverse(cfp, pfc);
	
	// atomic constraints
	forall(c in AtomicConstraints) {
		pfc[c.cbefore] < pfc[c.cafter]; 	
	}	
	
	// disjunctive constraints 
	forall(c in DisjunctiveConstraints) {
		pfc[c.c1before] < pfc[c.c1after] || pfc[c.c2before] < pfc[c.c2after]; 
		if(c.c1before == c.c2before) {
			maxl(pfc[c.c1after], pfc[c.c2after]) > pfc[c.c1before];
		}
	}
	
	// direct successor
	forall(i in DirectSuccessors: i<=b) {
	  	(pfc[i] < pfc[i+b]) => (pfc[i+b] - pfc[i] == 1);
	}

	forall(i in DirectSuccessors: i>b) {
	  	(pfc[i] < pfc[i-b]) => (pfc[i-b] - pfc[i] == 1);
	}
}
