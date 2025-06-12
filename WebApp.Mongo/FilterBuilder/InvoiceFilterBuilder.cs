using System.Globalization;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using WebApp.Enums;

namespace WebApp.Mongo.FilterBuilder;

public sealed class InvoiceFilterBuilder
{
    private string? _id;
    private string? _sellerTaxCode;
    private string? _buyerTaxCode;
    private string? _nameKeyword;
    private int? _invoiceNumber;
    private string? _khhdon;
    private int? _khmshdon;
    private string? _from;
    private string? _to;
    
    private InvoiceType? _invoiceType;

    private InvoiceStatus? _status;

    private bool? _risk;

    private InvoiceFilterBuilder()
    {
    }

    public static InvoiceFilterBuilder StartBuilder() => new();

    public FilterDefinition<T> Build<T>() where T : class
    {
        var filter = FilterDefinition<T>.Empty;
        if (!string.IsNullOrEmpty(_buyerTaxCode)) filter &= Builders<T>.Filter.Eq("nmmst", _buyerTaxCode);
        if (!string.IsNullOrEmpty(_sellerTaxCode)) filter &= Builders<T>.Filter.Eq("nbmst", _sellerTaxCode);
        if (!string.IsNullOrEmpty(_nameKeyword))
            filter &= Builders<T>.Filter.Or(
                Builders<T>.Filter.Regex("nbten", BsonRegularExpression.Create(
                                             new Regex(Regex.Escape(_nameKeyword), RegexOptions.IgnoreCase))),
                Builders<T>.Filter.Regex("nmten", BsonRegularExpression.Create(
                                             new Regex(Regex.Escape(_nameKeyword), RegexOptions.IgnoreCase)))
            );
        
        if (_invoiceNumber is not null) filter &= Builders<T>.Filter.Eq("shdon", _invoiceNumber.Value);
        
        if (_id is not null) filter &= Builders<T>.Filter.Eq("id", _id);
        
        if(_khhdon is not null) filter &= Builders<T>.Filter.Eq("khhdon", _khhdon);
        
        if(_khmshdon is not null) filter &= Builders<T>.Filter.Eq("khmshdon", _khmshdon);
        
        if (_from is not null && _to is not null)
        {
            filter &= Builders<T>.Filter.And(
                Builders<T>.Filter.Gte("tdlap", _from),
                Builders<T>.Filter.Lte("tdlap", _to)
            );
        }
        else if (_from is not null)
        {
            filter &= Builders<T>.Filter.Gte("tdlap", _from);
        }
        else if (_to is not null)
        {
            filter &= Builders<T>.Filter.Lte("tdlap", _to);
        }

        if (_invoiceType is not null)
        {
            filter &= Builders<T>.Filter.Eq("ttxly", _invoiceType.Value);
        }

        if (_status is not null)
        {
            filter &= Builders<T>.Filter.Eq("tthai", _status.Value);
        }

        if (_risk is not null)
        {
            if (_risk.Value == false)
            {
                filter &= Builders<T>.Filter.Or(
                    Builders<T>.Filter.Eq("risk", _risk.Value),
                    Builders<T>.Filter.Exists("risk", false),
                    Builders<T>.Filter.Eq("risk", false)
                );
            }
            else
            {
                filter &= Builders<T>.Filter.Eq("risk", _risk.Value);
            }
        }

        return filter;
    }

    public InvoiceFilterBuilder WithId(string? id)
    {
        _id = id;
        return this;
    }
    
    public InvoiceFilterBuilder HasNameKeyword(string? keyword)
    {
        _nameKeyword = keyword;
        return this;
    }

    public InvoiceFilterBuilder WithInvoiceNumber(int? number)
    {
        _invoiceNumber = number;
        return this;
    }

    public InvoiceFilterBuilder WithKhhdon(string? khhdon)
    {
        _khhdon = khhdon;
        return this;
    }
    
    public InvoiceFilterBuilder WithKhMshDon(int? khmshdon)
    {
        _khmshdon = khmshdon;
        return this;
    }

    public InvoiceFilterBuilder FromDate(string? from)
    {
        if (!DateTime.TryParseExact(from ?? string.Empty, "yyyy-MM-dd", null, DateTimeStyles.None, out var fromDate))
        {
            from = null;
        }

        _from = from is null
            ? null
            : fromDate.AddHours(-15)
                      .ToString("yyyy-MM-ddTHH:mm:ss");
        return this;
    }

    public InvoiceFilterBuilder ToDate(string? to)
    {
        if (!DateTime.TryParseExact(to ?? string.Empty, "yyyy-MM-dd", null, DateTimeStyles.None, out var toDate))
        {
            to = null;
        }

        _to = to is null
            ? null
            : toDate.AddHours(-15)
                    .AddHours(23)
                    .AddMinutes(59)
                    .AddSeconds(59)
                    .ToString("yyyy-MM-ddTHH:mm:ss");
        return this;
    }

    public InvoiceFilterBuilder WithStatus(InvoiceStatus? status)
    {
        _status = status;
        return this;
    }

    public InvoiceFilterBuilder WithRisk(bool? risk)
    {
        _risk = risk;
        return this;
    }

    public InvoiceFilterBuilder WithType(InvoiceType? type)
    {
        _invoiceType = type;
        return this;
    }

    public InvoiceFilterBuilder WithSeller(string? taxCode)
    {
        _sellerTaxCode = taxCode;
        return this;
    }

    public InvoiceFilterBuilder WithBuyer(string? taxCode)
    {
        _buyerTaxCode = taxCode;
        return this;
    }
}