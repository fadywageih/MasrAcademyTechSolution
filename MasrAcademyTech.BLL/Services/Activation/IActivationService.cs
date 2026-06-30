namespace MasrAcademyTech.BLL.Services.Activation
{
    public interface IActivationService
    {
        Task<string> GenerateCodeAsync(int courseId, string userId);
        Task<bool> ActivateCodeAsync(string code, string userId);
    }
}
