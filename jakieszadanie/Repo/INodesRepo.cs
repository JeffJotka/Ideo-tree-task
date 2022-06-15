using jakieszadanie.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jakieszadanie.Repo
{
   public interface INodesRepo
    {
        IQueryable<Node> GetNodes();
        Task AddNode(Node node);
        Task<Node> GetNodeByIdAsync(int? id);
        Task UpdateNodeAsync(Node node);
        bool NodeExists(int id);
        Task GetAllChildren(int parentId, List<Node> list);
        Task RemoveWithAllChildrenAsync(int id);
        Task<List<Node>> GetChildren(int ParentId);
        Task ChangeParent(int oldParentId, int newParentId);
        Task RemoveNodeAsync(Node node);
    }
}
