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
- Add comments in code
- Create project documentation
- Add logs
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

### Use of IRootFromHashCreator

Do we really have to live with the `IRootFromHashCreator` or we could remove it?

### Easy switch between data sources

Find a easy way to switch between CrewSchedule, Chinook or any other DataSource and Use Cases, e.g.

- Use a separate class for each data source & create a separate `IServiceProvider`
- Use different directory in `PhysicalFilesObjectStoreOptions`

### Use of LazyHashableObjectBase

Do we really have to live with the `LazyHashableObjectBase` or we could remove it?

### Generic implementation of ICommitsComparer

Find a way to create a generic implementation of the `ICommitsComparer` interface. Maybe we could add some `git4e` specific Attributes to content data, e.g.

- [ContentProperty] for properties
- [ContentCollection] for `HashableLists`

The generic implementation could use this metadata to find creations/updates/deletions etc

### Remove GetValue<> method calls

Add a new `GetValue()` method to each LazyXYZ class and use the new method.
For example see:

```csharp
public new Plan GetValue() => base.GetValue<Plan>();
```

Furthermore, we could rename these methods to `LoadValue` to imply that (probably) content loading will occur

### Remove PKs from included properties documentation

For example see:

```csharp
/// <summary>
/// Plan Hash with the following included properties:
/// 1. PlanVersionId
/// </summary>
public class LazyPlan : LazyHashableObject
```
