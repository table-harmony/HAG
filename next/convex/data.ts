import { v } from "convex/values";
import { mutation, query } from "./_generated/server";

export const updateImageData = mutation({
  args: { size: v.number(), count: v.number() },
  handler: async (ctx, args) => {
    const data = await ctx.db.query("images").first();

    if (!data) {
      await ctx.db.insert("images", {
        size: args.size,
        count: args.count,
      });
    } else {
      await ctx.db.patch(data._id, {
        size: data.size + args.size,
        count: data.count + args.count,
      });
    }
  },
});

export const getImageData = query({
  handler: async (ctx) => {
    const data = await ctx.db.query("images").first();

    return data;
  },
});

export const updateFileData = mutation({
  args: { size: v.number(), count: v.number() },
  handler: async (ctx, args) => {
    const data = await ctx.db.query("files").first();

    if (!data) {
      await ctx.db.insert("files", {
        size: args.size,
        count: args.count,
      });
    } else {
      await ctx.db.patch(data._id, {
        size: data.size + args.size,
        count: data.count + args.count,
      });
    }
  },
});

export const getFileData = query({
  handler: async (ctx) => {
    const data = await ctx.db.query("files").first();

    return data;
  },
});
