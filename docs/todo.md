# git4e

## Chinook Hierarchy

Library
    Artist
        Album
            Tracks (MediaType, Genre)

## Next steps

- έχουμε μία κλάση Globals που νομίζω πρέπει αυτά που έχει να προστεθούν στην κλάση/έννοια Repository. Δηλαδή όταν ανοιγεις/φτιάχνεις ένα repo, εκεί ξέρεις αν σώζεις σε folder ή σε άλλο object store (προς το παρόν δεν έχουμε κάποιο άλλο), αν παίζει με SHA1 ή άλλη IHashCalculator υλοποίηση κλπ
- Τώρα το PhysicalFilesObjectStore φτιάχνει έναν φάκελο objects και ένα αρχείο HEAD ενώ το "σωστό" (αντίστοιχα με αυτό που κάνει το git) θα είναι να έχουμε folder .git4e και μέσα να έχει HEAD file & objects folder  
- Τώρα όπως γράφουμε στο HEAD ουσιαστικά βρισκόμαστε σε detached head mode και δεν υπάρχει η έννοια branch. Να έχουμε στο HEAD δηλαδή ref στο checkedout branch που να δείχνει στο refs/heads/main κλπ. Μας λείπει δηλαδή
  - το api διαχείρισης branches
    - "git4e switch -c my_branch", "git  branch -d branch_to_delete" κλπ
  - initial branch (main/master)
- Στο git όταν αποθηκεύεις ένα tree υπολογίζεις το hash μιας πληροφορίας που έχει μέσα

> file1 hash_of_file1
> file2 hash_of_file2
> folder1 hash_of_file2

  πέρα απ το hash έχεις δηλαδή και το name του folder/file.
  Το αντίστοιχο στο git4e είναι να έχεις το PK του record
  Τωρα που ακομα δεν το έχουμε, δεν υπάρχει τρόπος να πεις πότε ένα record έγινε Insert/delete ή update

- Stabilize the API
- Add tests
- Add comments in code & documentation
- Version dlls & Publish nuget packages
- Find a killer-app that proves the validity of git4e

var version = SavePlan(insertedAssignments, updatedAssignments, deletedAssignments);
  -> trigger validations (version)
    var version2 = Save Plan
    -> trigger validations (version2)
    after 1 minute
      run validations(version)
        checkout plan(version);
          Select *? from seaman assignments
          Select *? from vessels
          Select *? from seamen
          Select *? from vesselPositions
          ...
    after 4 minute
      run validations(version2)
        checkout plan(version2);
          Select * from seaman assignments
          ...

Σύνολο από tables που ενδιαφέρεσαι να έχεις versions?

Νομική Βιβλιοθήκη
  Εφημερίδα Κυβ. Τεύχος 12002
    Νόμος
      +Παράγραφος
      -γραμμή

Νόμοι κράτους
  Νόμος
    Παράγραφος
      Εδάφιο
        Άρθρο
          text  
  Categories
    sa
      ddd

pf designer -> undo/redo
  enqueue/dequeue το json

{
  aa: [
  ],
  b: {
    c,
    d: {
      f,
      g
    }
  }
}

db

* να μιλήσουμε με Φιλιππόπουλο ή Βασιλάκη
  * Αν υπάρχει η ανάγκη, καλύπτεται αυτη τη στιγμή και πώς?

StockName, StockPrice, When
AMZN, 123,76$, 2021-...
AMZN, 123,86$, 2021-...
MSFT, 153,76$, 2021-...

Commit
  Plan
    Vessels
      VesselPositions
        Assignments
          Seamam

- Ιδέες Βασιλάκη
  - save logs for multiple tables
