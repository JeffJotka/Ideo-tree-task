using jakieszadanie.Data;
using jakieszadanie.Models;
using jakieszadanie.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace jakieszadanie.Controllers
{
    public class HomeController : Controller
    {
        private readonly INodesRepo _NodesRepo;
        public HomeController(ApplicationDbContext context, INodesRepo nodesRepo)
        {
            _NodesRepo = nodesRepo;
        }
        public IActionResult Index()
        {
            List<Node> list = _NodesRepo.GetNodes().Include(node => node.Children).AsEnumerable().Where(node => node.ParentId == null).ToList();
            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            List<Node> nodes = await _NodesRepo.GetNodes().OrderBy(node => node.Name).ToListAsync();
            if (nodes.Count() != 0)
            {
                ViewData["ParentId"] = new SelectList(nodes, "Id", "Name");
                ViewBag.HasRoot = true;
            }
            else ViewBag.HasRoot = false;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ParentId")] Node node)
        {
            if (ModelState.IsValid)
            {
                await _NodesRepo.AddNode(node);
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentId"] = new SelectList(await _NodesRepo.GetNodes().OrderBy(node => node.Name).ToListAsync(), "Id", "Name", node.ParentId);
            return View(node);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id < 0) return NotFound();
            var node = await _NodesRepo.GetNodeByIdAsync(id);
            if (node == null) return NotFound();
            List<Node> nodes = await _NodesRepo.GetNodes().OrderBy(n => n.Name).Where(n => n.Id != node.Id).ToListAsync();
            List<Node> allchildren = new List<Node>();
            await _NodesRepo.GetAllChildren(node.Id, allchildren);
            nodes = nodes.Except(allchildren).ToList();
            ViewData["ParentId"] = new SelectList(nodes, "Id", "Name", node.ParentId);
            ViewBag.hasChildren = false;
            if (await _NodesRepo.GetNodes().AnyAsync(n => n.ParentId == node.Id))
            {
                ViewBag.hasChildren = true;
            }
            return View(node);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ParentId")] Node node)
        {
            if (id != node.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    await _NodesRepo.UpdateNodeAsync(node);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_NodesRepo.NodeExists(id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentId"] = new SelectList(await _NodesRepo.GetNodes().OrderBy(n => n.Name).Where(n => n.Id != node.Id).ToListAsync(), "Id", "Name", node.ParentId);
            return View(node);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _NodesRepo.RemoveWithAllChildrenAsync(id);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteAndMove(int? id)
        {
            if (id == null || id < 0) return NotFound();
            var node = await _NodesRepo.GetNodeByIdAsync(id);
            if (node == null) return NotFound();
            ViewData["Parent"] = node.Name;
            List<Node> children = await _NodesRepo.GetChildren(id.Value);
            ViewBag.Children = children;
            List<Node> nodes = await _NodesRepo.GetNodes().OrderBy(n => n.Name).Where(n => n.Id != node.Id).ToListAsync();
            nodes = nodes.Except(children).ToList();
            ViewBag.NodesSelectList = new SelectList(nodes, "Id", "Name");
            DelAndMove dt = new DelAndMove() { NodeId = id.Value };
            return View(dt);
        }
        [HttpPost, ActionName("DeleteAndMove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAndMoveConfirmed(int id, [Bind("NodeId, TargetId")] DelAndMove dt)
        {
            if (id != dt.NodeId || id < 0) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    await _NodesRepo.ChangeParent(dt.NodeId, dt.TargetId);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!_NodesRepo.NodeExists(dt.TargetId)) return NotFound();
                    else throw;
                }
                var node = await _NodesRepo.GetNodeByIdAsync(dt.NodeId);
                if (node == null) return NotFound();
                await _NodesRepo.RemoveNodeAsync(node);
                return RedirectToAction(nameof(Index));
            }
            return View(dt);
        }
    }
}
