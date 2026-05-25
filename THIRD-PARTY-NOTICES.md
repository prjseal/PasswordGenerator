# Third-Party Notices

This project (PasswordGenerator) is licensed under the MIT License (see
`License.md`). It also includes third-party data, listed below, that is
distributed under its own license.

## EFF Large Wordlist

`PasswordGenerator/WordList.cs` embeds the **EFF Large Wordlist** (7,776 words),
created by the **Electronic Frontier Foundation** (Joseph Bonneau) and published at:

- https://www.eff.org/dice
- https://www.eff.org/files/2016/07/18/eff_large_wordlist.txt

**License:** Creative Commons Attribution 3.0 United States (CC BY 3.0 US)
https://creativecommons.org/licenses/by/3.0/us/

**Modifications:** The original tab-separated `dice-number<TAB>word` rows were
reformatted into a C# `string[]` array. The words themselves are unchanged.

The EFF Large Wordlist is used here to generate diceware-style passphrases.
