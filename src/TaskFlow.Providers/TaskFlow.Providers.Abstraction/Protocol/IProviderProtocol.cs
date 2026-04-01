namespace TaskFlow.Providers.Abstraction.Protocol;

public interface IProviderProtocol
{
    string SerializeCreateNextTaskRequest(CreateNextTaskRequest request);

    string SerializeCreateNextTaskResponse(CreateNextTaskResponse response);

    CreateNextTaskResponse ParseCreateNextTaskResponse(string json);

    string SerializeReviewTaskRequest(ReviewTaskRequest request);

    string SerializeReviewTaskResponse(ReviewTaskResponse response);

    ReviewTaskResponse ParseReviewTaskResponse(string json);

    string SerializeExecuteTaskRequest(ExecuteTaskRequest request);

    string SerializeExecuteTaskResponse(ExecuteTaskResponse response);

    ExecuteTaskResponse ParseExecuteTaskResponse(string json);
}
