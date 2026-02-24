using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class JudgeRepository : IJudgeRepository
{
    private readonly IMongoCollection<Judge> _collection;

    public JudgeRepository(WineMongoDbContext context) =>
        _collection = context.Judges;

    public IList<Judge> GetAllJudges() =>
        _collection.Find(_ => true).SortBy(j => j.Name).ToList();

    public Judge? GetJudgeById(string id) =>
        _collection.Find(j => j.JudgeId == id).FirstOrDefault();

    public string AddJudge(Judge judge)
    {
        _collection.InsertOne(judge);
        return judge.JudgeId;
    }

    public void DeleteJudge(string id) =>
        _collection.DeleteOne(j => j.JudgeId == id);
}
