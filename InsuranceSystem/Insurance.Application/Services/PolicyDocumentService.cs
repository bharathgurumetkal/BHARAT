using System;
using System.Text;
using System.Threading.Tasks;
using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace Insurance.Application.Services
{
    public class PolicyDocumentService : IPolicyDocumentService
    {
        private readonly IPolicyService _policyService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IPropertyRepository _propertyRepository;

        public PolicyDocumentService(
            IPolicyService policyService, 
            ICustomerRepository customerRepository, 
            IPropertyRepository propertyRepository)
        {
            _policyService = policyService;
            _customerRepository = customerRepository;
            _propertyRepository = propertyRepository;
            
            // Set QuestPDF License
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GeneratePolicyScheduleAsync(Guid policyId, string docType = "Schedule")
        {
            var policy = await _policyService.GetPolicyByIdAsync(policyId);
            if (policy == null) throw new Exception("Policy not found.");

            var customer = await _customerRepository.GetByIdAsync(policy.CustomerId);
            var property = await _propertyRepository.GetByIdAsync(policy.PropertyId);
            
            var customerName = customer?.User?.Name ?? "Valued Customer";
            var customerEmail = customer?.User?.Email ?? "N/A";
            var customerPhone = customer?.User?.PhoneNumber ?? "N/A";

            // Local style functions
            static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
            static IContainer ContentStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Helvetica"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(t => t.Span("PROPERTYTRUST INSURANCE").FontSize(22).SemiBold().FontColor(Colors.Blue.Medium));
                            col.Item().Text(t => t.Span("Trusted Protection for Your Most Valuable Assets").FontSize(9).Italic().FontColor(Colors.Grey.Medium));
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text(t => t.Span(docType == "Coverage" ? "COVERAGE SUMMARY" : "POLICY SCHEDULE").FontSize(12).SemiBold().FontColor(Colors.Grey.Darken2));
                            col.Item().Text(t => t.Span($"Ref: PT-{policy.PolicyNumber.Substring(0, 8).ToUpper()}").FontSize(9).FontColor(Colors.Grey.Medium));
                            col.Item().Text(t => t.Span($"Date: {DateTime.Now:dd MMM yyyy}").FontSize(9));
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Spacing(20);

                        // Basic Info Row
                        x.Item().Row(row =>
                        {
                            row.RelativeItem().Column(inner => {
                                inner.Item().Text(t => t.Span("POLICY IDENTIFICATION").FontSize(9).SemiBold().FontColor(Colors.Blue.Medium));
                                inner.Item().PaddingTop(2).Text(t => t.Span(policy.PolicyNumber).FontSize(11).Bold());
                                inner.Item().Text(t => t.Span(policy.ProductName).FontSize(9).FontColor(Colors.Grey.Medium));
                            });
                            row.RelativeItem().Column(inner => {
                                inner.Item().Text(t => t.Span("INSURED PARTY").FontSize(9).SemiBold().FontColor(Colors.Blue.Medium));
                                inner.Item().PaddingTop(2).Text(t => t.Span(customerName).FontSize(11).Bold());
                                inner.Item().Text(t => t.Span(customerEmail).FontSize(9).FontColor(Colors.Grey.Medium));
                            });
                        });

                        if (docType == "Coverage")
                        {
                            // Detailed Coverage Table
                            x.Item().Column(inner => {
                                inner.Item().PaddingBottom(5).BorderBottom(1).BorderColor(Colors.Blue.Lighten2).Text(t => t.Span("DETAILED COVERAGE BENEFITS").FontSize(12).SemiBold().FontColor(Colors.Blue.Medium));
                                inner.Item().PaddingTop(10).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    // Header
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("Coverage Group");
                                        header.Cell().Element(CellStyle).Text("Benefit Limit");
                                        header.Cell().Element(CellStyle).Text("Status");
                                    });

                                    // Content
                                    table.Cell().Element(ContentStyle).Text("Fire & Lightning Damage");
                                    table.Cell().Element(ContentStyle).Text("100% of Sum Insured");
                                    table.Cell().Element(ContentStyle).Text(t => t.Span("INCLUDED").FontColor(Colors.Green.Medium).SemiBold());

                                    table.Cell().Element(ContentStyle).Text("Flood & Natural Calamities");
                                    table.Cell().Element(ContentStyle).Text("100% of Sum Insured");
                                    table.Cell().Element(ContentStyle).Text(t => t.Span("INCLUDED").FontColor(Colors.Green.Medium).SemiBold());

                                    table.Cell().Element(ContentStyle).Text("Theft, Burglary & Larceny");
                                    table.Cell().Element(ContentStyle).Text("₹ " + (policy.CoverageAmount * 0.8m).ToString("N0"));
                                    table.Cell().Element(ContentStyle).Text(t => t.Span("INCLUDED").FontColor(Colors.Green.Medium).SemiBold());

                                    table.Cell().Element(ContentStyle).Text("Third Party Liability");
                                    table.Cell().Element(ContentStyle).Text("₹ 5,00,000 Fix");
                                    table.Cell().Element(ContentStyle).Text(t => t.Span("OPTIONAL EXTRA").FontColor(Colors.Blue.Medium).SemiBold());
                                });
                            });

                            x.Item().Column(inner => {
                                inner.Item().Text(t => t.Span("IMPORTANT EXCLUSIONS").FontSize(10).SemiBold().FontColor(Colors.Red.Medium));
                                
                                void AddBullet(string text) => inner.Item().Row(r => { 
                                    r.AutoItem().Text("• "); 
                                    r.RelativeItem().Text(text).FontSize(9); 
                                });

                                AddBullet("Damage due to wear and tear or gradual deterioration.");
                                AddBullet("Damage caused by war, invasion, or nuclear radiation.");
                                AddBullet("Loss of cash, bullion, or jewelry unless specifically endorsed.");
                            });
                        }
                        else
                        {
                            // Property Section for Schedule
                            if (property != null)
                            {
                                x.Item().Column(inner => {
                                    inner.Item().PaddingBottom(5).BorderBottom(1).BorderColor(Colors.Blue.Lighten2).Text(t => t.Span("PROPERTY RISK DETAILS").FontSize(12).SemiBold().FontColor(Colors.Blue.Medium));
                                    inner.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                        });

                                        table.Cell().PaddingVertical(5).Text(t => t.Span("Physical Address:").SemiBold());
                                        table.Cell().PaddingVertical(5).Text(property.Address);
                                        
                                        table.Cell().PaddingVertical(5).Text(t => t.Span("Asset Classification:").SemiBold());
                                        table.Cell().PaddingVertical(5).Text($"{property.Category} - {property.SubCategory}");
                                        
                                        table.Cell().PaddingVertical(5).Text(t => t.Span("Construction Year:").SemiBold());
                                        table.Cell().PaddingVertical(5).Text(property.YearBuilt.ToString());

                                        table.Cell().PaddingVertical(5).Text(t => t.Span("Risk Zone Assessment:").SemiBold());
                                        table.Cell().PaddingVertical(5).Text(t => t.Span(property.RiskZone).SemiBold().FontColor(property.RiskZone == "High" ? Colors.Red.Medium : Colors.Green.Medium));
                                    });
                                });
                            }
                        }

                        // Financial Summary (Always shown)
                        x.Item().Background(Colors.Grey.Lighten4).Padding(15).Column(inner =>
                        {
                            inner.Item().Text(t => t.Span("TOTAL SUM INSURED").FontSize(8).SemiBold().FontColor(Colors.Grey.Medium));
                            inner.Item().Row(row =>
                            {
                                row.RelativeItem().Text(t => t.Span($"₹ {policy.CoverageAmount:N0}").FontSize(20).Bold().FontColor(Colors.Blue.Medium));
                                row.RelativeItem().AlignRight().Column(c => {
                                    c.Item().Text(t => t.Span("ANNUAL PREMIUM").FontSize(8).SemiBold().FontColor(Colors.Grey.Medium));
                                    c.Item().Text(t => t.Span($"₹ {policy.Premium:N0}").FontSize(16).Bold());
                                });
                            });
                        });

                        // Validity
                        x.Item().PaddingTop(10).Text(t => {
                            t.Span("Coverage Period: ").SemiBold();
                            t.Span($"{policy.StartDate:dd MMM yyyy} to {policy.EndDate:dd MMM yyyy}");
                        });

                        // Compliance
                        x.Item().PaddingTop(20).Text(t => t.Span("This certificate is generated by the PropertyTrust Automated Issuance System and is legally valid for all insurance claims and legal verification. Any alteration to this document is a punishable offense under the Digital Protection Act.").FontSize(7).FontColor(Colors.Grey.Medium).LineHeight(1.5f));
                    });

                    page.Footer().AlignCenter().Column(col => {
                        col.Item().PaddingBottom(5).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2);
                        col.Item().Row(row => {
                            row.RelativeItem().Text(t => t.Span("propertytrust.com | Support: 1800-PROPERTY").FontSize(8).FontColor(Colors.Grey.Medium));
                            row.RelativeItem().AlignRight().DefaultTextStyle(s => s.FontSize(8)).Text(x => {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                            });
                        });
                        col.Item().AlignCenter().Text(t => t.Span($"Verified Electronic Document - Generated on {DateTime.Now:f}").FontSize(7).FontColor(Colors.Grey.Lighten1));
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
