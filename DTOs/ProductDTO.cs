

namespace DTOs
{
    public record ProductDTO(

     int ProductId,

     string ProductName,

     decimal Price,

     int CategoryId,

     string Description,

     string ImageUrl,

     bool IsAvailable
        );


}