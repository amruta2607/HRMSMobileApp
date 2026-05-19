using MobileWebApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace MobileWebApi.Helper
{
	public static class SalarySlipPdfGenerator
	{
		public static byte[] Generate(PaySlipDetail model)
		{
			QuestPDF.Settings.License = LicenseType.Community;

			string monthName = model.PayrollMonth >= 1 && model.PayrollMonth <= 12
				? new DateTime(model.PayrollYear, model.PayrollMonth, 1).ToString("MMMM")
				: "";

			if (model.PayrollMonth >= 4)
				model.FinancialYearStart = model.PayrollYear;
			else
				model.FinancialYearStart = model.PayrollYear - 1;

			return Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Size(PageSizes.A4);
					page.Margin(20);

					page.DefaultTextStyle(x =>
						x.FontFamily("Calibri")
						 .FontSize(11)
					);

					page.Content().Column(column =>
					{
						column.Spacing(10);

						// ================= HEADER =================
						column.Item().Border(1).Padding(5).Row(row =>
						{
							row.RelativeItem().AlignMiddle().AlignCenter().Column(col =>
							{
								col.Item().AlignCenter()
									.Text(model.TenantName ?? "")
									.FontSize(14)
									.Bold();

								col.Item().AlignCenter()
									.Text($"Monthly Salary Statement {monthName} {model.PayrollYear}")
									.FontSize(13)
									.Bold();

								col.Item().AlignCenter()
									.Text($"Financial Period {model.FinancialYearStart}-{model.FinancialYearStart + 1}")
									.FontSize(12)
									.Bold();
							});
						});


						// ================= EMPLOYEE INFO =================
						column.Item().Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.ConstantColumn(120);
								columns.RelativeColumn();
								columns.ConstantColumn(120);
								columns.RelativeColumn();
							});

							void Cell(string label, string value)
							{
								table.Cell().Border(0.5f).Padding(4).Text(label);
								table.Cell().Border(0.5f).Padding(4).Text(value ?? "");
							}

							table.Cell().ColumnSpan(4)
								.Background(Colors.Grey.Lighten3)
								.Padding(4)
								.Text("Employee Information")
								.Bold()
								.AlignCenter();

							table.Cell().ColumnSpan(4)
								.Padding(4)
								.Text(model.EmployeeName ?? "")
								.Bold();

							Cell("Employee Id", model.EmployeeNumber);
							Cell("Branch/Site", model.BranchName);

							Cell("Designation", model.DesignationName);
							Cell("PAN", model.TaxNumber);

							Cell("Gender", model.GenderName);
							Cell("Bank A/C", model.BankAccountNumber);

							Cell("Date Of Joining", model.DateOfJoining?.ToString("dd-MMM-yyyy"));
							Cell("Days Payable", model.DaysPayable.ToString());

							Cell("Date Of Birth", model.DateOfBirth?.ToString("dd-MMM-yyyy"));
							Cell("Overtime Days", model.OverTimeDays.ToString());

							Cell("PF A/C", model.PFNo);
							Cell("Present Days", model.PresentDays.ToString());

							Cell("ESI A/C", model.ESINo);
							Cell("Weekly Off Days", model.TotalWeekOffDays.ToString());

							Cell("UAN No", model.UANNo);
							Cell("Absent Days", (model.DaysPayable - model.PresentDays).ToString());
						});

						// ================= EARNINGS & DEDUCTIONS =================
						column.Item().Row(row =>
						{
							// ===== EARNINGS =====
							row.RelativeItem().Border(1).Padding(5).Table(table =>
							{
								table.ColumnsDefinition(columns =>
								{
									columns.RelativeColumn();
									columns.ConstantColumn(100);
								});

								table.Cell().Background(Colors.Grey.Lighten3).Padding(3)
									.Text("Earnings").Bold().AlignCenter();
								table.Cell().Background(Colors.Grey.Lighten3).Padding(3)
									.Text("Amount").Bold().AlignCenter();

								foreach (var income in model.Earnings ?? Enumerable.Empty<PaySlipLineItem>())
								{
									table.Cell().Border(0.5f).Padding(3)
										.Text(income.Description);
									table.Cell().Border(0.5f).Padding(3)
										.AlignRight()
										.Text(income.Amount.ToString("#,##0.00"));
								}

								table.Cell().Height(50);
								table.Cell().Height(50);

								table.Cell().Border(0.5f).Padding(3)
									.Text("(A) Total Earnings").Bold();
								table.Cell().Border(0.5f).Padding(3)
									.AlignRight()
									.Text(model.Gross.ToString("#,##0.00")).Bold();
							});

							// ===== DEDUCTIONS =====
							row.RelativeItem().Border(1).Padding(5).Table(table =>
							{
								table.ColumnsDefinition(columns =>
								{
									columns.RelativeColumn();
									columns.ConstantColumn(100);
								});

								table.Cell().Background(Colors.Grey.Lighten3).Padding(3)
									.Text("Deductions").Bold().AlignCenter();
								table.Cell().Background(Colors.Grey.Lighten3).Padding(3)
									.Text("Amount").Bold().AlignCenter();

								foreach (var deduction in model.Deductions ?? Enumerable.Empty<PaySlipLineItem>())
								{
									table.Cell().Border(0.5f).Padding(3)
										.Text(deduction.Description);
									table.Cell().Border(0.5f).Padding(3)
										.AlignRight()
										.Text(deduction.Amount.ToString("#,##0.00"));
								}

								table.Cell().Height(50);
								table.Cell().Height(50);

								table.Cell().Border(0.5f).Padding(3)
									.Text("(B) Total Deductions").Bold();
								table.Cell().Border(0.5f).Padding(3)
									.AlignRight()
									.Text(model.TotalDeduction.ToString("#,##0.00")).Bold();
							});
						});

						// ================= NET SALARY =================
						column.Item().Border(1).Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn();
								columns.ConstantColumn(120);
							});

							table.Cell().Padding(5)
								.AlignRight()
								.Text("Net Salary = (A) – (B)").Bold();

							table.Cell().Padding(5)
								.AlignRight()
								.Text(model.TakeHomePay.ToString("#,##0.00")).Bold();
						});

						// ================= FOOTER =================
						column.Item().Border(1).Padding(5).Column(col =>
						{
							col.Item().Text("This is a computer-generated slip; therefore, no signature is required.")
								.FontSize(9);
							col.Item().Height(80);
						});
					});
				});
			}).GeneratePdf();
		}
	}
}
