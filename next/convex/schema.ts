import { defineSchema, defineTable } from "convex/server";
import { v } from "convex/values";

export default defineSchema({
  images: defineTable({
    size: v.number(),
    count: v.number(),
  }),
  files: defineTable({
    size: v.number(),
    count: v.number(),
  }),
});
