# git4e

## Chinook Hierarchy

Library
    Artist
        Album
            Tracks (MediaType, Genre)

## Next steps

- ~~έχουμε μία κλάση Globals που νομίζω πρέπει αυτά που έχει να προστεθούν στην κλάση/έννοια Repository. Δηλαδή όταν ανοιγεις/φτιάχνεις ένα repo, εκεί ξέρεις αν σώζεις σε folder ή σε άλλο object store (προς το παρόν δεν έχουμε κάποιο άλλο), αν παίζει με SHA1 ή άλλη IHashCalculator υλοποίηση κλπ~~
- ~~Τώρα το PhysicalFilesObjectStore φτιάχνει έναν φάκελο objects και ένα αρχείο HEAD ενώ το "σωστό" (αντίστοιχα με αυτό που κάνει το git) θα είναι να έχουμε folder .git4e και μέσα να έχει HEAD file & objects folder~~
- ~~Τώρα όπως γράφουμε στο HEAD ουσιαστικά βρισκόμαστε σε detached head mode και δεν υπάρχει η έννοια branch. Να έχουμε στο HEAD δηλαδή ref στο checkedout branch που να δείχνει στο refs/heads/main κλπ. Μας λείπει δηλαδή~~
  - το api διαχείρισης branches
    - "git4e switch -c my_branch", "git  branch -d branch_to_delete" κλπ
      - Status update: partially implemented
  - ~~initial branch (main/master)~~
- ~~Στο git όταν αποθηκεύεις ένα tree υπολογίζεις το hash μιας πληροφορίας που έχει μέσα~~

> ~~file1 hash_of_file1~~
> ~~file2 hash_of_file2~~
> ~~folder1 hash_of_file2~~

  ~~πέρα απ το hash έχεις δηλαδή και το name του folder/file.
  Το αντίστοιχο στο git4e είναι να έχεις το PK του record
  Τωρα που ακομα δεν το έχουμε, δεν υπάρχει τρόπος να πεις πότε ένα record έγινε Insert/delete ή update~~

- Stabilize the API
- Add tests
  - Status update: some tests added
- Add comments in code & documentation
- Version dlls & Publish nuget packages
- Find a killer-app that proves the validity of git4e

## Issues

### Signatures

Add smth like libgit2sharp [Signature](https://github.com/libgit2/libgit2sharp/blob/master/LibGit2Sharp/Signature.cs) and use it for [Author](https://github.com/libgit2/libgit2sharp/blob/df3b22a754ef56da8d7e3c330ce2d783c2b7982e/LibGit2Sharp/Commit.cs#L82) and [Commiter](https://github.com/libgit2/libgit2sharp/blob/df3b22a754ef56da8d7e3c330ce2d783c2b7982e/LibGit2Sharp/Commit.cs#L87) Commit properties

### Storing HashableLists

Could we remove the use of 2 properties for storing HashableLists?

```csharp
[ProtoMember(3)]
public string AlbumsHash { get; set; }
[ProtoMember(4)]
public string[] AlbumFullHashes { get; set; }
```
