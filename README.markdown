The Shopify API Wrapper for .NET 4.0
=====================================

Shopify is great for running a store - but up until now there haven't been many API tools for .NET. This simple library is useful for querying your Shopify store as well as adding, editing, and deleting items.

How To Install It?
------------------
Reference the Shopify.dll in your project.

How Do You Use It?
------------------
This might come as a big surprise to people... but it's all dynamic. You structure queries based on what you need from the API.

For instance - if you want to query for products, you make a JSON request to the Shopify API (the docs are here: http://api.shopify.com/product.html):

	var shopify = new Shopify.Api("somestore.myshopify.com","myAPIKey","myAPIPassword");
	var query = shopify.Products();
	foreach(var product in query.products){
		Console.Writeline(product.title);
	}

In this example, the method "Products()" doesn't exist - it's "caught" by the API wrapper and a URL constructed for you (with the necessary credentials). What you get back is a dynamic bit of JSON that you can then loop over. You'll need to know the structure of the return call - but it's all documented up in the Shopify API.

You can also query by passing in parameters:
	
	var shopify = new Shopify.Api("somestore.myshopify.com","myAPIKey","myAPIPassword");
	var query = shopify.Products(collection_id: 12121212);
	foreach(var product in query.products){
		Console.Writeline(product.title);
	}

Same thing - the call here is dynamicall inferred based on the name of the named argument.

Adding, Editing, Deleting
-------------------------
Again, all dynamic. For this you use an ExpandoObject and pass it into the API:
	
	//use an Anon type here if you like... it just needs to match the casing and structure of the Shopify bits
	var p = new {
		title = "My new Product";
		handle = "sex-appeal";
		//add variants and price here using arrays
	}

	var shopify = new Shopify.Api("somestore.myshopify.com","myAPIKey","myAPIPassword");
	shopify.Products.Save(p);

The last call there is, once again, dynamically inferred. "Products" doesn't exist as a property on "shopify" - so a bunch of goodness happens which results in you being able to execute an HTTP POST to the Shopify API.

If there's an "id" present on the product, a "PUT" will be executed, which will update the item (Product, Collection, Blog, Page... whatever).

To delete, you simply need to know the ID. You can do that easily:
	
	var shopify = new Shopify.Api("somestore.myshopify.com","myAPIKey","myAPIPassword");
	var query = shopify.Products(handle: "sex-appeal");
	var id = query.products[0].id
	shopify.Products.Delete(id);
