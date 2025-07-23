using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

class Item
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }

    public double TotalPrice()
    {
        return Quantity * UnitPrice;
    }
}

class Program
{
    static void Main(string[] args)
    {

        QuestPDF.Settings.License = LicenseType.Community;
        List<Item> cart = new List<Item>();

        while (true)
        {
            Console.WriteLine("\n=== Invoice Generator ===");
            Console.WriteLine("1. Add Item");
            Console.WriteLine("2. View Bill");
            Console.WriteLine("3. Generate Invoice (PDF)");
            Console.WriteLine("4. Exit");
            Console.Write("Choose an option: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddItem(cart);
                    break;
                case "2":
                    ViewBill(cart);
                    break;
                case "3":
                    GenerateInvoice(cart);
                    GeneratePdfInvoice(cart);
                    break;
                case "4":
                    Console.WriteLine("Thank you for using the Invoice Generator. Goodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }
        }
    }

    static void AddItem(List<Item> cart)
    {
        Console.Write("Enter item name: ");
        string name = Console.ReadLine();

        Console.Write("Enter quantity: ");
        if (!int.TryParse(Console.ReadLine(), out int qty))
        {
            Console.WriteLine("Invalid quantity.");
            return;
        }

        Console.Write("Enter unit price: ");
        if (!double.TryParse(Console.ReadLine(), out double price))
        {
            Console.WriteLine("Invalid price.");
            return;
        }

        Item newItem = new Item
        {
            Name = name,
            Quantity = qty,
            UnitPrice = price
        };

        cart.Add(newItem);
        Console.WriteLine("Item added successfully!");
    }

    static void ViewBill(List<Item> cart)
    {
        if (cart.Count == 0)
        {
            Console.WriteLine("Cart is empty.");
            return;
        }

        Console.WriteLine("\nCurrent Items in Cart:");
        Console.WriteLine("Name\tQty\tUnit Price\tTotal");

        foreach (Item item in cart)
        {
            Console.WriteLine($"{item.Name}\t{item.Quantity}\t{item.UnitPrice:F2}\t\t{item.TotalPrice():F2}");
        }
    }

    static void GenerateInvoice(List<Item> cart)
    {
        if (cart.Count == 0)
        {
            Console.WriteLine("No items to generate invoice.");
            return;
        }

        double subtotal = cart.Sum(i => i.TotalPrice());
        double tax = subtotal * 0.18;
        double grandTotal = subtotal + tax;

        Console.WriteLine("\n=========== INVOICE ===========");
        Console.WriteLine("Item\tQty\tUnit Price\tTotal");

        foreach (Item item in cart)
        {
            Console.WriteLine($"{item.Name}\t{item.Quantity}\t{item.UnitPrice:F2}\t\t{item.TotalPrice():F2}");
        }

        Console.WriteLine("----------------------------------");
        Console.WriteLine($"Subtotal:\t\t\t{subtotal:F2}");
        Console.WriteLine($"Tax (18%):\t\t\t{tax:F2}");
        Console.WriteLine($"Grand Total:\t\t\t{grandTotal:F2}");
        Console.WriteLine("==================================");
    }

    static void GeneratePdfInvoice(List<Item> cart)
    {
        double subtotal = cart.Sum(item => item.TotalPrice());
        double tax = subtotal * 0.18;
        double grandTotal = subtotal + tax;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header()
                    .Text("INVOICE")
                    .FontSize(24)
                    .SemiBold()
                    .AlignCenter();

                page.Content().Column(col =>
                {
                    col.Item().Text("Purchased Items").FontSize(16).Bold();

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(80);
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Item");
                            header.Cell().Element(CellStyle).Text("Qty");
                            header.Cell().Element(CellStyle).Text("Price");
                            header.Cell().Element(CellStyle).Text("Total");
                        });

                        // Items
                        foreach (var item in cart)
                        {
                            table.Cell().Element(CellStyle).Text(item.Name);
                            table.Cell().Element(CellStyle).Text(item.Quantity.ToString());
                            table.Cell().Element(CellStyle).Text(item.UnitPrice.ToString("F2"));
                            table.Cell().Element(CellStyle).Text(item.TotalPrice().ToString("F2"));
                        }

                        static IContainer CellStyle(IContainer container)
                        {
                            return container
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(5)
                                .PaddingHorizontal(2);
                        }
                    });

                    col.Item().PaddingTop(20).Text($"Subtotal: ₹{subtotal:F2}");
                    col.Item().Text($"Tax (18%): ₹{tax:F2}");
                    col.Item().Text($"Grand Total: ₹{grandTotal:F2}").Bold().FontSize(14);
                });

                page.Footer().AlignCenter().Text("Thank you for shopping with us!");
            });
        });

        document.GeneratePdf("Invoice.pdf");
        Console.WriteLine("✅ PDF invoice saved as 'Invoice.pdf'");
    }
}
