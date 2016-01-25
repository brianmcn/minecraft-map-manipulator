module RecomputeLighting

// recompute the BlockLight/SkyLight/HeightMap/LightPopulated values after making changes

// need opacity table for all blocks
// need table of what blocks emit light at what level    BLOCKIDS_THAT_EMIT_LIGHT
// need to be careful with leaves/webs & skylight
// need to be careful that slabs/stairs/farmland are opaque but have self light-level of neighbor

let INTRINSIC_BRIGHTNESS =
    let a = Array.zeroCreate 256
    for bid, ib in MC_Constants.BLOCKIDS_THAT_EMIT_LIGHT do
        a.[bid] <- ib
    a

let brightness(bid) = INTRINSIC_BRIGHTNESS.[bid]

let opacity(bid) =
    if bid=0uy then 1   // TODO table of all blocks
    else 15



(*

blocklight:

have set of 'could be changed by update' (easy to compute at start).  
all blocks with intrinsic light values there are noted, as well as all points on the surface boundary, as light 'sources' with strength N

then for N = 15 downto 0 do

forall sources with strength N, set block light to N (unless already higher, in which case stop), 
then add all neighbors to lists of N-minus-their-opacity sources (unless they were already brighter)

thus we have the 15 wavefront, then the 14 wavefront, ... no work is done for e.g. red torches until we reach iter 7

each block is only ever set once

-----------------------

skylight: all blocks above heightmap become 15-sources (ensure all sections above HM are represented), then propogate wavefront

-----------------------

the 'could be changed by update' set is useful for a single change or a set of changes
to recompute entire chunk/region at once, probably best to just do everything, e.g. the 'could be changed' is 'everything'
hm, do we need the 'could be changed' set? does the wavefront implicitly do this?
aha, yes, we need if for dimming - when light sources are dimmed or removed, we need to
 - compute the changed set (the N-radius around the N-light-removed block)
 - note its frontier (set on the boundary/edge of the region)
 - after unioning all the changed sets from all the changes, union the frontiers and then for each guy on the frontier,
     check all his neighbors for blocks outside the change scope, these become the new 'sources' that will bleed in

-----------------------

we can address all the light arrays relatively quickly (O(1)) via the region files (need to code up the direct access)
each section is 2048 bytes of BlockLight
each chunk is up to 16*2048 bytes
each region is up to 32*32*16*2048 = 33,554,432 bytes = 32MB of memory just for the BlockLight info (but we've been holding 16 regions in memory fine with other stuff)

-----------------------

skylight is similar to blocklight
everything above HM is a source of 15
some special work for leaves/cobwebs
then basic lighting computation
any time a block is placed above HM, need to fix HM, and also ensure sections exist from bottom up to that block, recompute
any time a block is removed at HM, need to fix HM, recompute

*)


