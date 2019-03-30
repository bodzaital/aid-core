using System;
using System.Collections.Generic;
using System.Text;

namespace aid_core2
{
	class Product
	{
		public string Name { get; set; }
		public double Price { get; set; }
		public string Description { get; set; }

		public Product(string name, string price, string desription)
		{
			Name = name;
			Price = Convert.ToDouble(price.Substring(1, price.IndexOf('.') + 2).Replace('.', ','));
			Description = Description;
		}

		public override string ToString()
		{
			return $"Name: {Name}, Price: {Price}, Description: {Description}";
		}
	}
}
