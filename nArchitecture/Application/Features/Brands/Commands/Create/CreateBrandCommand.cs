using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Brands.Commands.Create;

public class CreateBrandCommand : IRequest<CreatedBrandResponse>
{
    public string Name { get; set; }



	public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, CreatedBrandResponse>
	{

		private readonly IBradRepository _brandRepository;
		private readonly  IMapper _mapper;

		public CreateBrandCommandHandler(IBradRepository brandRepository, IMapper mapper)
		{
			_brandRepository = brandRepository;
			_mapper = mapper;
		}

		public async Task<CreatedBrandResponse>? Handle(CreateBrandCommand request, CancellationToken cancellationToken)
		{


			Brand brnd = _mapper.Map<Brand>(request);
			brnd.Id= Guid.NewGuid();
			 await _brandRepository.AddAsync(brnd);

			CreatedBrandResponse createdBrandResponse = _mapper.Map<CreatedBrandResponse>(brnd);
			
			return createdBrandResponse;
		}
	}
}
