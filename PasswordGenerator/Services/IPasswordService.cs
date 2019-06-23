namespace PasswordGenerator.Services
{
    public interface IPasswordService
    {
        IPasswordService IncludeLowercase();
        IPasswordService IncludeUppercase();
        IPasswordService IncludeNumeric();
        IPasswordService IncludeSpecial();
        IPasswordService LengthRequired(int passwordLength);
        string Next();       
    }
}