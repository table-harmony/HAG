import { v } from "convex/values";
import { mutation, query } from "./_generated/server";

export const updateData = mutation({
  args: { size: v.number(), count: v.number() },
  handler: async (ctx, args) => {
    const data = await ctx.db.query("data").first();

    if (!data) {
      await ctx.db.insert("data", {
        size: args.size,
        count: args.count,
      });
    } else {
      await ctx.db.patch(data?._id, {
        size: data?.size + args.size,
        count: data?.count + args.count,
      });
    }
  },
});

export const getData = query({
  handler: async (ctx) => {
    const data = await ctx.db.query("data").first();

    return data;
  },
});
