using WineApp.Models;

namespace WineApp.Data;

public interface IJudgeRepository
{
    IList<Judge> GetAllJudges();
    Judge? GetJudgeById(string id);
    string AddJudge(Judge judge);
    void DeleteJudge(string id);
}
