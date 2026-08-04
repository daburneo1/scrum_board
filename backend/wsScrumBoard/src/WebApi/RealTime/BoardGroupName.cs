namespace WebApi.RealTime;

internal static class BoardGroupName
{
    public static string FromProjectId(Guid projectId)
    {
        return $"board:{projectId:N}";
    }
}