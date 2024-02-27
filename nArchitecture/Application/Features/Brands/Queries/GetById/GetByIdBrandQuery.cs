using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Brands.Queries.GetById;

public class GetByIdBrandQuery : IRequest<GetByIdBrandResponse>
{
	public Guid Id { get; set; }

	public class GetByIdBrandResponseHandler : IRequestHandler<GetByIdBrandQuery, GetByIdBrandResponse>
	{
		private readonly IMapper _mapper;
		private readonly IBrandRepository _bradRepository;

		public GetByIdBrandResponseHandler(IMapper mapper, IBrandRepository bradRepository)
		{
			_mapper = mapper;
			_bradRepository = bradRepository;
		}

		public async Task<GetByIdBrandResponse> Handle(GetByIdBrandQuery request, CancellationToken cancellationToken)
		{
			Brand? brand=  await _bradRepository.GetAsync(predicate:b=> b.Id == request.Id,cancellationToken:cancellationToken);

			GetByIdBrandResponse response= _mapper.Map<GetByIdBrandResponse>(brand);
			return response;

		}
	}

	
}
