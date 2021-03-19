# Contributing [Shamisen](https://github.com/MineCake147E/Shamisen/)

## Issues
- The Issues should be described in English or Japanese.
  - When an issue is written in Japanese, the owner can translate it in order to discuss in English.
  - When an issue is written in another language, the owner **MUST** close it as invalid one.
### Found Bugs
- Write some minimal representation code first.
- Create a new Issue with [Bug Report](https://github.com/MineCake147E/Shamisen/issues/new?assignees=&labels=&template=bug_report.md&title=) Template

### Feature Request(WIP)
- Make sure your proposal doesn't collide any existing features.
  - Feel free to suggest some syntax sugar like fatures.
- Create a new Issue with [Feature Request](https://github.com/MineCake147E/Shamisen/issues/new?assignees=&labels=&template=feature_request.md&title=) Template.

## Coding(WIP)
- You should not commit directly to branches.
- Use [Pull Request](https://github.com/MineCake147E/Shamisen/compare) instead.
### Code Rules(WIP)
#### All Text files(except for auto-generated non-C# files)
| Name | Value |
| -- | -- |
|NewLine|CRLF|
|Encoding|UTF-8|

#### C\#
- ***DO NOT USE `T[] buffer, int offset, int count` patterns at ANY places!***(when possible)
  - Range checks can be remained under this pattern!
  - You **MUST** use `Span<T>`, `Memory<T>`, `ReadOnlySpan<T>`, `ReadOnlyMemory<T>` except for some needed places.
- **Both `Unsafe` and `MemoryMarshal` should NEVER BE AVOIDED!**  
  -  Don't forget that `Span<T>` can wrap any pointers thoguh.
- Manage memories yourself if appropriate.
- Be careful when using `MemoryMarshal.Cast<T, (T2,T3,T4,T5)>(Span<T> span)` because `StructLayout` of `ValueTuple` is set to `Auto`.

#### Comments
- Write XML comments at **EVERY** public members.
- The comments should be written in English except for automatically-generated comments like `IDisposable` patterns.

### Pull Requests(WIP)
