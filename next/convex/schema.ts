import { defineSchema, defineTable } from "convex/server";
import { v } from "convex/values";

export default defineSchema({
  data: defineTable({
    size: v.number(),
    count: v.number(),
  }),
});
