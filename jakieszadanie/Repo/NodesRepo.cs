using jakieszadanie.Data;
using jakieszadanie.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jakieszadanie.Repo
{
    public class NodesRepo : INodesRepo
    {
        private readonly ApplicationDbContext _context;

        public NodesRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Node> GetNodes()
        {
            return _context.Nodes.AsQueryable();
        }
        public async Task AddNode(Node node)
        {
            await _context.Nodes.AddAsync(node);
            await _context.SaveChangesAsync();
        }
        public async Task<Node> GetNodeByIdAsync(int? id)
        {
            var node = await _context.Nodes.FirstOrDefaultAsync(node => node.Id == id);
            return node;
        }
        public async Task UpdateNodeAsync(Node node)
        {
            _context.Nodes.Update(node);
            await _context.SaveChangesAsync();
        }
        public bool NodeExists(int id)
        {
            return _context.Nodes.Any(node => node.Id == id);
        }
        public async Task GetAllChildren(int parentId, List<Node> list)
        {
            List<Node> children = await GetNodes().Where(nd => nd.ParentId == parentId).ToListAsync();
            if (children.Count != 0)
            {
                list.AddRange(children);
                foreach (var child in children)
                {
                    await GetAllChildren(child.Id, list);
                }
            }
        }
        public async Task RemoveWithAllChildrenAsync(int id)
        {
            var node = await _context.Nodes.FindAsync(id);
            List<Node> toRemove = new List<Node>() { node };
            await GetAllChildren(id, toRemove);
            _context.Nodes.RemoveRange(toRemove);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Node>> GetChildren(int ParentId)
        {
            return await GetNodes().Where(nd => nd.ParentId == ParentId).ToListAsync();
        }

        public async Task ChangeParent(int oldParentId, int newParentId)
        {
            var children = await GetChildren(oldParentId);
            foreach (var child in children)
            {
                child.ParentId = newParentId;
            }
            _context.Nodes.UpdateRange(children);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveNodeAsync(Node node)
        {
            _context.Nodes.Remove(node);
            await _context.SaveChangesAsync();
        }
    }
}

