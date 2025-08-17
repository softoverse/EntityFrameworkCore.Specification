using System.Linq.Expressions;

using Benchmark.DataAccess;
using Benchmark.Helpers;
using Benchmark.Models;

using BenchmarkDotNet.Attributes;

using Microsoft.EntityFrameworkCore;

using Softoverse.EntityFrameworkCore.Specification.Abstraction;
using Softoverse.EntityFrameworkCore.Specification.Helpers;
using Softoverse.EntityFrameworkCore.Specification.Implementation;

namespace Benchmark;

[MemoryDiagnoser]
public class MyBenchmark
{
    private ApplicationDbContext _context;
    private IRepositoryBase<Article, long> _repository;

    private string _q = "Hospital";
    private string _title = "Hospital";

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                      .UseInMemoryDatabase(databaseName: "TestDatabase")
                      .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ArticleRepository(_context);

        // Seed data
        var hospitals = new List<Article>();
        for (int i = 0; i < 100; i++)
        {
            hospitals.Add(new Article
            {
                Id = i + 1,
                TitleEn = "TitleEn" + $" Hospital {i}",
                TitleBn = "TitleEn" + $" Hospital {i}",
                TitleAr = "TitleEn" + $" Hospital {i}",
                TitleHi = "TitleEn" + $" Hospital {i}",
                ShortDescriptionEn = "ShortDescriptionEn" + $" Hospital {i}",
                ShortDescriptionBn = "ShortDescriptionEn" + $" Hospital {i}",
                ShortDescriptionAr = "ShortDescriptionEn" + $" Hospital {i}",
                ShortDescriptionHi = "ShortDescriptionEn" + $" Hospital {i}",
                DescriptionEn = "DescriptionEn" + $" Hospital {i}",
                DescriptionBn = "DescriptionEn" + $" Hospital {i}",
                DescriptionAr = "DescriptionEn" + $" Hospital {i}",
                DescriptionHi = "DescriptionEn" + $" Hospital {i}",
                ImageUrl = "ImageUrl"
            });
        }
        _context.Set<Article>().AddRange(hospitals);
        _context.SaveChanges();
    }

    [Benchmark]
    public async Task<List<Article>> GetHospitalsWithDbContext()
    {
        Expression<Func<Article, bool>> predicate = x => true;

        if (string.IsNullOrEmpty(_q) is false)
        {
            Expression<Func<Article, bool>> qPredicates = a => EF.Functions.Like(a.TitleEn, $"%{_q}%")
                                                             ||
                                                               EF.Functions.Like(a.TitleBn, $"%{_q}%")
                                                             ||
                                                               EF.Functions.Like(a.TitleAr, $"%{_q}%")
                                                             ||
                                                               EF.Functions.Like(a.TitleHi, $"%{_q}%")
                                                             ||
                                                               EF.Functions.Like(a.DescriptionEn, $"%{_q}%")
                                                             ||
                                                               EF.Functions.Like(a.DescriptionBn, $"%{_q}%")
                                                             ||
                                                               EF.Functions.Like(a.DescriptionAr, $"%{_q}%")
                                                             ||
                                                               EF.Functions.Like(a.DescriptionHi, $"%{_q}%");

            predicate = predicate is null
                ? qPredicates
                : predicate.AndAlso(qPredicates); // Combine using helper below
        }

        if (string.IsNullOrEmpty(_title) is false)
        {
            Expression<Func<Article, bool>> titlePredicate = a =>
                a.TitleEn == _title ||
                a.TitleBn == _title ||
                a.TitleAr == _title ||
                a.TitleHi == _title;

            predicate = predicate is null
                ? titlePredicate
                : predicate.AndAlso(titlePredicate); // Combine using helper below
        }

        return await _context.Articles.Where(predicate).ToListAsync();
    }

    // [Benchmark]
    // public async Task<List<Article>> GetHospitalsWithDbContext2()
    // {
    //     Expression<Func<Article, bool>> predicate = x => true;
    //
    //     if (string.IsNullOrEmpty(_q) is false)
    //     {
    //         Expression<Func<Article, bool>> qPredicates = a => EF.Functions.Like(a.TitleEn, $"%{_q}%")
    //                                                          ||
    //                                                            EF.Functions.Like(a.TitleBn, $"%{_q}%")
    //                                                          ||
    //                                                            EF.Functions.Like(a.TitleAr, $"%{_q}%")
    //                                                          ||
    //                                                            EF.Functions.Like(a.TitleHi, $"%{_q}%")
    //                                                          ||
    //                                                            EF.Functions.Like(a.DescriptionEn, $"%{_q}%")
    //                                                          ||
    //                                                            EF.Functions.Like(a.DescriptionBn, $"%{_q}%")
    //                                                          ||
    //                                                            EF.Functions.Like(a.DescriptionAr, $"%{_q}%")
    //                                                          ||
    //                                                            EF.Functions.Like(a.DescriptionHi, $"%{_q}%");
    //
    //         predicate = predicate is null
    //             ? qPredicates
    //             : predicate.AndAlso(qPredicates); // Combine using helper below
    //     }
    //
    //     if (string.IsNullOrEmpty(_title) is false)
    //     {
    //         Expression<Func<Article, bool>> titlePredicate = a =>
    //             a.TitleEn == _title ||
    //             a.TitleBn == _title ||
    //             a.TitleAr == _title ||
    //             a.TitleHi == _title;
    //
    //         predicate = predicate is null
    //             ? titlePredicate
    //             : predicate.AndAlso(titlePredicate); // Combine using helper below
    //     }
    //
    //     return await _context.Articles.Where(predicate).ToListAsync();
    // }

    [Benchmark]
    public async Task<List<Article>> GetHospitalsWithSpecification()
    {
        var spec = new GetArticlesSpecification(_q, _title);
        return await _repository.GetAllAsync(spec.GetSpecification(false, true));
    }

    // [Benchmark]
    // public async Task<List<Article>> GetHospitalsWithSpecification2()
    // {
    //     var spec = new GetArticlesSpecification(_q, _title);
    //     return await _repository.GetAllAsync(spec.GetSpecification(false, true));
    // }

    [Benchmark]
    public async Task<List<Article>> GetHospitalsWithoutExpressionBuilder()
    {
        var spec = new GetArticlesSpecificationWithoutExpressionBuilder(_q, _title);
        return await _repository.GetAllAsync(spec.GetSpecification(false, true));
    }

    // [Benchmark]
    // public async Task<List<Article>> GetHospitalsWithoutExpressionBuilder2()
    // {
    //     var spec = new GetArticlesSpecificationWithoutExpressionBuilder(_q, _title);
    //     return await _repository.GetAllAsync(spec.GetSpecification(false, true));
    // }

    [Benchmark]
    public async Task<List<Article>> GetHospitalsWithoutSpecificationWithCriteria()
    {
        var spec = new GetArticlesSpecificationWithoutExpressionBuilder(_q, _title);
        return await _repository.GetAllAsync(spec.GetSpecification(false, true).Criteria);
    }

    // [Benchmark]
    // public async Task<List<Article>> GetHospitalsWithoutSpecificationWithCriteria2()
    // {
    //     var spec = new GetArticlesSpecificationWithoutExpressionBuilder(_q, _title);
    //     return await _repository.GetAllAsync(spec.GetSpecification(false, true).Criteria);
    // }
}

public class GetArticlesSpecification : ISpecificationRequest<Article>
{
    private readonly string _q;
    private readonly string _title;

    public GetArticlesSpecification(string? q, string? title)
    {
        _q = q;
        _title = title;
    }

    public ISpecification<Article> GetSpecification(bool asNoTracking = false, bool asSplitQuery = true)
    {
        List<Expression<Func<Article, bool>>> expressions = [];

        if (_q is not null)
        {
            List<Expression<Func<Article, bool>>> qExpressions =
            [
                Specification<Article>.ToConditionalExpression(p => p.TitleEn, _q, x => EF.Functions.Like(x.TitleEn, $"%{_q}%")),
                Specification<Article>.ToConditionalExpression(p => p.TitleBn, _q, x => EF.Functions.Like(x.TitleBn, $"%{_q}%")),
                Specification<Article>.ToConditionalExpression(p => p.TitleAr, _q, x => EF.Functions.Like(x.TitleAr, $"%{_q}%")),
                Specification<Article>.ToConditionalExpression(p => p.TitleHi, _q, x => EF.Functions.Like(x.TitleHi, $"%{_q}%")),

                Specification<Article>.ToConditionalExpression(p => p.DescriptionEn, _q, x => EF.Functions.Like(x.DescriptionEn, $"%{_q}%")),
                Specification<Article>.ToConditionalExpression(p => p.DescriptionBn, _q, x => EF.Functions.Like(x.DescriptionBn, $"%{_q}%")),
                Specification<Article>.ToConditionalExpression(p => p.DescriptionAr, _q, x => EF.Functions.Like(x.DescriptionAr, $"%{_q}%")),
                Specification<Article>.ToConditionalExpression(p => p.DescriptionHi, _q, x => EF.Functions.Like(x.DescriptionHi, $"%{_q}%")),
            ];

            expressions.Add(qExpressions.CombineWithOr());
        }

        if (_title is not null)
        {
            List<Expression<Func<Article, bool>>> nameExpressions =
            [
                Specification<Article>.ToConditionalExpression(p => p.TitleEn, _title, EqualOperation.Equal),
                Specification<Article>.ToConditionalExpression(p => p.TitleBn, _title, EqualOperation.Equal),
                Specification<Article>.ToConditionalExpression(p => p.TitleAr, _title, EqualOperation.Equal),
                Specification<Article>.ToConditionalExpression(p => p.TitleHi, _title, EqualOperation.Equal)
            ];

            expressions.Add(nameExpressions.CombineWithOr());
        }

        Specification<Article> specification = new(expressions, CombineType.And, asNoTracking: asNoTracking, asSplitQuery: asSplitQuery)
        {
            OrderByExpression = p => p.Id
        };

        return specification;
    }
}

public class GetArticlesSpecificationWithoutExpressionBuilder : ISpecificationRequest<Article>
{
    private readonly string _q;
    private readonly string _title;

    public GetArticlesSpecificationWithoutExpressionBuilder(string? q, string? title)
    {
        _q = q;
        _title = title;
    }

    public ISpecification<Article> GetSpecification(bool asNoTracking = false, bool asSplitQuery = true)
    {
        List<Expression<Func<Article, bool>>> qExpressions = [];
        List<Expression<Func<Article, bool>>> tExpressions = [];
        List<Expression<Func<Article, bool>>> expressions = [];

        if (_q is not null)
        {
            qExpressions.Add(x => EF.Functions.Like(x.TitleEn, $"%{_q}%"));
            qExpressions.Add(x => EF.Functions.Like(x.TitleBn, $"%{_q}%"));
            qExpressions.Add(x => EF.Functions.Like(x.TitleAr, $"%{_q}%"));
            qExpressions.Add(x => EF.Functions.Like(x.TitleHi, $"%{_q}%"));

            qExpressions.Add(x => EF.Functions.Like(x.DescriptionEn, $"%{_q}%"));
            qExpressions.Add(x => EF.Functions.Like(x.DescriptionBn, $"%{_q}%"));
            qExpressions.Add(x => EF.Functions.Like(x.DescriptionAr, $"%{_q}%"));
            qExpressions.Add(x => EF.Functions.Like(x.DescriptionHi, $"%{_q}%"));

            expressions.Add(qExpressions.CombineWithOr());
        }

        if (_title is not null)
        {
            tExpressions.Add(x => x.TitleEn == _title);
            tExpressions.Add(x => x.TitleBn == _title);
            tExpressions.Add(x => x.TitleAr == _title);
            tExpressions.Add(x => x.TitleHi == _title);

            expressions.Add(tExpressions.CombineWithOr());
        }

        Specification<Article> specification = new(expressions, CombineType.And, asNoTracking: asNoTracking, asSplitQuery: asSplitQuery)
        {
            OrderByExpression = p => p.Id
        };

        return specification;
    }
}