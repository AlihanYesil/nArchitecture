using Core.Persistence.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Responses;

public class GetListResponse <T> : BasePageableModel
{
	private  List<T> _items;

	public List<T> Items
	{ 
		get => _items??= new List<T>(); 
		set => _items = value; 
	}
}
