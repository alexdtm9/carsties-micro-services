using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
        var auctions = await _context.Auctions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();

        return Ok(_mapper.Map<List<AuctionDto>>(auctions));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuction(Guid id)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();

        return Ok(_mapper.Map<AuctionDto>(auction));
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        auction.Seller = "Placeholder seller";

        _context.Auctions.Add(auction);
        if (await _context.SaveChangesAsync() > 0) BadRequest("Could not save auction.");

        return CreatedAtAction(nameof(GetAuction), new { id = auction.Id }, _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        if (await _context.SaveChangesAsync() > 0) return NoContent();

        return BadRequest("Could not update auction.");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();

        _context.Remove(auction);
        if (await _context.SaveChangesAsync() > 0) return BadRequest("Could not delete action");

        return Ok();
    }
}