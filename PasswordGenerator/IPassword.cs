using System.Collections.Generic;

namespace PasswordGenerator
{
    public interface IPassword
    {
        IPassword IncludeLowercase();
        IPassword IncludeUppercase();
        IPassword IncludeNumeric();
        IPassword IncludeSpecial();
        IPassword LengthRequired(int passwordLength);
        string Next();
        IEnumerable<string> NextGroup(int numberOfPasswordsToGenerate);
    }
}