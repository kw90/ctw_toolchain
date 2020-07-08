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


range Positions = 0..(k-1);
int bigM = k*100; // Big M - upper bound for constraint functions


dvar float+ S;
dvar float+ L; 
dvar float+ M;
dvar int N;

// variables for constraints
dvar float+ pfc[Cavities] in Positions; 
dvar float+ minimum[Cablestarts] in Positions; // min{pfc[i],pfc[i+b]}
dvar float+ maximum[Cablestarts] in Positions; // max{pfc[i],pfc[i+b]}

dvar boolean lt[Cavities][Cavities]; // indicator whether pfc[i]<pfc[j]
dvar boolean cableIsStored[Cablestarts]; // indicator whether cable (i,j) requires storage
dvar boolean cableIsStoredAtPosition[Cablestarts,Positions]; // indicator whether cable (i,i+b) requires storage at position p
dvar boolean cableIsPluggedBefore[Cablestarts,Positions]; // indicator whether i and i+b are plugged before t
dvar boolean cableIsPluggedAfter[Cablestarts,Positions]; // indicator whether i and i+b are plugged after t

minimize S * pow(k, 3)
		   + M * pow(k, 2)
		   + L * pow(k, 1) 
		   + N;
		   
subject to {

	S == sum(p in Cablestarts) cableIsStored[p]; // number of cables requiring storage
 	N == sum(s in SoftAtomicConstraints) (pfc[s.cafter] - pfc[s.cbefore] <= 0);

// L = min{N | storage time <= N for all cables}
	forall(p in Cablestarts)
	  L_in_memory:
	  maximum[p] - minimum[p] - 1 <= L;

// M = min{N | storage occupation <= N for all Positions}
	forall(t in Positions)
	  M_consumption:
	  sum(p in Cablestarts) cableIsStoredAtPosition[p,t] <= M;

// all different
	forall(ordered i,j in Cavities)
		all_different1:
        pfc[i] - pfc[j] + 1 <= bigM*(1 - lt[i,j]);
	forall(ordered i,j in Cavities)
		all_different2:
        pfc[j] - pfc[i] + 1 <= bigM*lt[i,j];

// cableIsStored[i,j]=1 <=> cable (i,j) requires storage
	forall(p in Cablestarts)
		requires_storage1:
		2 - maximum[p] + minimum[p] <= bigM*(1 - cableIsStored[p]);
	forall(p in Cablestarts)
		requires_storage2:
        maximum[p] - minimum[p] - 1 <= bigM*cableIsStored[p];

// cableIsStoredAtPosition[i,j,t]=1 <=> cable (i,j) requires storage at time t
	forall(p in Cablestarts, t in Positions)
		requires_storage_at_time_t1:
		minimum[p] - t + 1 <= bigM*(1 - cableIsStoredAtPosition[p,t]);
	forall(p in Cablestarts, t in Positions)
		requires_storage_at_time_t2:
		t - maximum[p] + 1 <= bigM*(1 - cableIsStoredAtPosition[p,t]);
	forall(p in Cablestarts, t in Positions)
		requires_storage_at_time3:
        t - minimum[p] <= bigM*(1-cableIsPluggedBefore[p,t]);
	forall(p in Cablestarts, t in Positions)
		requires_storage_at_time_t4:
        maximum[p] - t <= bigM*(1-cableIsPluggedAfter[p,t]);
	forall(p in Cablestarts, t in Positions)
		requires_storage_at_time_t5:
		cableIsStoredAtPosition[p,t] + cableIsPluggedBefore[p,t] + cableIsPluggedAfter[p,t] == 1;

	// min/max constraints
	forall(i in Cablestarts)
		is_lower_bound1:
		minimum[i] - pfc[i] <= 0;
	forall(i in Cablestarts)
		is_lower_bound2:
		minimum[i] - pfc[i+b] <= 0;
	forall(i in Cablestarts)
		minimum_is_first:
		pfc[i] - minimum[i] <= bigM*(1-lt[i][i+b]);
	forall(i in Cablestarts)
		minimum_is_last:
		pfc[i+b] - minimum[i] <= bigM*lt[i,i+b];

	forall(i in Cablestarts)
		set_maximum:
		maximum[i] == pfc[i]+pfc[i+b] - minimum[i];

	// direct successor constraints
	forall(i in Cablestarts: i in DirectSuccessors || (i+b) in DirectSuccessors)
		{
		direct_successor_constraints:
		if (i in DirectSuccessors)
			{pfc[i+b] - pfc[i] - 1 <= bigM*(1 - lt[i, i+b]);}
		if ((i+b) in DirectSuccessors)
			{pfc[i] - pfc[i+b] - 1 <= bigM*lt[i,i+b];}
		}	
				
	// AtomicConstraints
	// lt[i,j] is defined for i<j only
	forall(a in AtomicConstraints)
		if (a.cbefore < a.cafter)
			{
				lt[a.cbefore,a.cafter] == 1;
			}
		else
			lt[a.cafter,a.cbefore] == 0;

// disjunctive constraints
// In the following we use that for boolean variables a,b we have
// a+b >= 1 <=> a==1 or b==1
forall(d in DisjunctiveConstraints)
	disjunctions:
	{
	if (d.c1before < d.c1after
		&& d.c2before < d.c2after)
			{lt[d.c1before,d.c1after] 
				+ lt[d.c2before,d.c2after] 
				>= 1;}
	if (d.c1before < d.c1after
		&& d.c2before > d.c2after)
			{lt[d.c1before,d.c1after] 
				+ 1 - lt[d.c2after,d.c2before] 
				>= 1;}
	if (d.c1before > d.c1after
		&& d.c2before < d.c2after)
			{1 - lt[d.c1after,d.c1before] 
				+ lt[d.c2before,d.c2after] 
				>= 1;}
	if (d.c1before > d.c1after
		&& d.c2before > d.c2after)
			{1 - lt[d.c1after,d.c1before] 
				+ 1 - lt[d.c2after,d.c2before]
				>= 1;}
	}
}