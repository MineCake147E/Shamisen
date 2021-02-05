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
#### C\#
- ***DO NOT USE ANY ~~EXTREMELY SUCKING~~ `T[] buffer, int offset, int count` patterns!***
  - Range checks can be remained under this pattern!
  - You **MUST** use `Span<T>`, `Memory<T>`, `ReadOnlySpan<T>`, `ReadOnlyMemory<T>` except for some needed places.
- **RELY YOUR TESTED `unsafe` CODES FOR OPTIMIZATION!** 
  -  Don't forget that `Span<T>` can wrap any pointers thoguh.
- Consider Latencies of filters.
  - Don't forget to seal your filter classes if not needed to be `abstract` or `virtual`.
- Manage memories yourself if possible and appropriate.

#### Comments
- Write XML comments at **EVERY** public members.
- The comments should be written in English except for automatically-generated comments like `IDisposable` patterns.

### Pull Requests(WIP)
