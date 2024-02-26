using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class InternalServerErrorProblemDetails : ProblemDetails
{
	public InternalServerErrorProblemDetails(string detail)
	{
		Title = "InternalServerError";
		Detail = "InternalServerError";
		Status = StatusCodes.Status500InternalServerError;
		Type = "https://example.com/probs/business";
	}
}
